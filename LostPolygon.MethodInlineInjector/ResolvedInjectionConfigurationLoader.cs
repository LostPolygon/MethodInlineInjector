using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using devtm.Cecil.Extensions;
using LostPolygon.MethodInlineInjector.Configuration;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectionConfigurationLoader {
        private readonly Dictionary<string, AssemblyDefinitionData> _assemblyPathToAssemblyDefinitionMap = new Dictionary<string, AssemblyDefinitionData>();
        private readonly InjectionConfiguration _injectionConfiguration;

        public static ResolvedInjectionConfiguration LoadFromInjectionConfiguration(InjectionConfiguration injectionConfiguration) {
            return new ResolvedInjectionConfigurationLoader(injectionConfiguration).Load();
        }

        protected ResolvedInjectionConfigurationLoader(InjectionConfiguration injectionConfiguration) {
            _injectionConfiguration = injectionConfiguration;
        }

        public ResolvedInjectionConfiguration Load() {
            Dictionary<AssemblyDefinition, List<ResolvedInjectionConfiguration.InjectedMethod>> injectedAssemblyToMethodsMap = GetInjectedMethods();
            
            List<ResolvedInjectionConfiguration.InjectedAssemblyMethods> injectedAssemblyMethods = new List<ResolvedInjectionConfiguration.InjectedAssemblyMethods>();
            foreach (KeyValuePair<AssemblyDefinition, List<ResolvedInjectionConfiguration.InjectedMethod>> pair in injectedAssemblyToMethodsMap) {
                injectedAssemblyMethods.Add(new ResolvedInjectionConfiguration.InjectedAssemblyMethods(pair.Key, pair.Value.AsReadOnly()));
            }

            List<ResolvedInjectionConfiguration.InjecteeAssembly> injecteeAssemblies = GetInjecteeAssemblies();

            Validate(injectedAssemblyMethods, injecteeAssemblies);

            ResolvedInjectionConfiguration resolvedInjectionConfiguration = 
                new ResolvedInjectionConfiguration(
                    injectedAssemblyMethods.AsReadOnly(), 
                    injecteeAssemblies.AsReadOnly()
                    );
            return resolvedInjectionConfiguration;
        }

        private void Validate(
            List<ResolvedInjectionConfiguration.InjectedAssemblyMethods> injectedAssemblyMethods, 
            List<ResolvedInjectionConfiguration.InjecteeAssembly> injecteeAssemblies
            ) {
            AssemblyDefinition minInjecteeTargetRuntimeAssemblyDefinition = null;
            AssemblyDefinition maxInjectedTargetRuntimeAssemblyDefinition = null;
            foreach (ResolvedInjectionConfiguration.InjectedAssemblyMethods injectedAssembly in injectedAssemblyMethods) {
                AssemblyDefinition assemblyDefinition = injectedAssembly.AssemblyDefinition;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (maxInjectedTargetRuntimeAssemblyDefinition == null || targetRuntime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    maxInjectedTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            foreach (ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                AssemblyDefinition assemblyDefinition = injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (minInjecteeTargetRuntimeAssemblyDefinition == null || targetRuntime > minInjecteeTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    minInjecteeTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            if (maxInjectedTargetRuntimeAssemblyDefinition == null)
                throw new InvalidOperationException(nameof(maxInjectedTargetRuntimeAssemblyDefinition) + " == null");

            foreach (ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                if (injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.MainModule.Runtime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    throw new MethodInlineInjectorException(
                        $"Injectee assembly '{injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition}' " +
                        $"uses runtime version {injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.MainModule.Runtime}, " +
                        $"but assembly '{maxInjectedTargetRuntimeAssemblyDefinition}' used in injection uses runtime " +
                        $"version {maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime}."
                        );        
                }
            }
        }

        private Dictionary<AssemblyDefinition, List<ResolvedInjectionConfiguration.InjectedMethod>> GetInjectedMethods() {
            var injectedAssemblyToMethodsMap = new Dictionary<AssemblyDefinition, List<ResolvedInjectionConfiguration.InjectedMethod>>();
            foreach (InjectionConfiguration.InjectedMethod sourceInjectedMethod in _injectionConfiguration.InjectedMethods) {
                AssemblyDefinitionData assemblyDefinitionData = GetAssemblyDefinitionData(sourceInjectedMethod.AssemblyPath);
                MethodDefinition[] matchingMethodDefinitions =
                    assemblyDefinitionData.AllMethods
                        .Where(methodDefinition => methodDefinition.GetFullName() == sourceInjectedMethod.MethodFullName)
                        .ToArray();

                if (matchingMethodDefinitions.Length == 0)
                    throw new MethodInlineInjectorException($"No matching methods found for {sourceInjectedMethod.MethodFullName}");

                if (matchingMethodDefinitions.Length > 2)
                    throw new MethodInlineInjectorException($"More than 1 matching method found for {sourceInjectedMethod.MethodFullName}");

                List<ResolvedInjectionConfiguration.InjectedMethod> methodDefinitions;
                if (!injectedAssemblyToMethodsMap.TryGetValue(assemblyDefinitionData.AssemblyDefinition, out methodDefinitions)) {
                    methodDefinitions = new List<ResolvedInjectionConfiguration.InjectedMethod>();
                    injectedAssemblyToMethodsMap.Add(assemblyDefinitionData.AssemblyDefinition, methodDefinitions);
                }

                MethodDefinition matchedMethodDefinition = matchingMethodDefinitions[0];
                if (!ValidateInjectedMethod(matchedMethodDefinition))
                    throw new MethodInlineInjectorException($"Method {matchedMethodDefinition} can not be an injected method");

                methodDefinitions.Add(new ResolvedInjectionConfiguration.InjectedMethod(sourceInjectedMethod, matchedMethodDefinition));
            }

            return injectedAssemblyToMethodsMap;
        }

        private AssemblyDefinitionData GetAssemblyDefinitionData(string assemblyPath) {
            assemblyPath = Path.GetFullPath(assemblyPath);
            if (!_assemblyPathToAssemblyDefinitionMap.TryGetValue(assemblyPath, out AssemblyDefinitionData assemblyDefinitionData)) {
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
                assemblyDefinitionData = new AssemblyDefinitionData(assemblyDefinition);
                _assemblyPathToAssemblyDefinitionMap.Add(assemblyPath, assemblyDefinitionData);
            }

            return assemblyDefinitionData;
        }

        private List<ResolvedInjectionConfiguration.InjecteeAssembly> GetInjecteeAssemblies() {
            var injecteeAssemblies = new List<ResolvedInjectionConfiguration.InjecteeAssembly>();
            foreach (InjectionConfiguration.InjecteeAssembly sourceInjecteeAssembly in _injectionConfiguration.InjecteeAssemblies) {
                ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly = GetInjecteeAssembly(sourceInjecteeAssembly);
                injecteeAssemblies.Add(injecteeAssembly);
            }

            return injecteeAssemblies;
        }

        private ResolvedInjectionConfiguration.InjecteeAssembly GetInjecteeAssembly(InjectionConfiguration.InjecteeAssembly sourceInjecteeAssembly) {
            var memberReferenceWhitelistFilters = new List<InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter>();
            foreach (InjectionConfiguration.InjecteeAssembly.IMemberReferenceWhitelistItem memberReferenceWhitelistItem in sourceInjecteeAssembly.MemberReferenceWhitelist) {
                if (memberReferenceWhitelistItem is InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter memberReferenceWhitelistFilter) {
                    memberReferenceWhitelistFilters.Add(memberReferenceWhitelistFilter);
                    continue;
                }

                if (memberReferenceWhitelistItem is InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilterInclude memberReferenceWhitelistFilterInclude) {
                    string whiteListIncludeXml = File.ReadAllText(memberReferenceWhitelistFilterInclude.Path);
                    MemberReferenceWhitelistFilterInclude memberReferenceWhitelistFilterIncludedList = SimpleXmlSerializationUtility.XmlDeserializeFromString<MemberReferenceWhitelistFilterInclude>(whiteListIncludeXml);
                    memberReferenceWhitelistFilters.AddRange(memberReferenceWhitelistFilterIncludedList.MemberReferenceWhitelist);
                }
            }

            AssemblyDefinitionData assemblyDefinitionData = GetAssemblyDefinitionData(sourceInjecteeAssembly.AssemblyPath);
            List<MethodDefinition> injecteeMethodDefinitions = new List<MethodDefinition>();

            // Skip non-injectable methods
            List<MethodDefinition> filteredMethods = 
                assemblyDefinitionData
                .AllMethods
                .ToList();

            // TODO: implement whitelist filtering
            filteredMethods = FilterInjecteeMethods(filteredMethods);
            injecteeMethodDefinitions.AddRange(filteredMethods);

            ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly = 
                new ResolvedInjectionConfiguration.InjecteeAssembly(sourceInjecteeAssembly, assemblyDefinitionData, injecteeMethodDefinitions);

            return injecteeAssembly;
        }

        protected virtual List<MethodDefinition> FilterInjecteeMethods(List<MethodDefinition> injecteeMethods) {
            return 
                injecteeMethods
                .Where(ValidateInjecteeMethod)
                .ToList();
        }

        protected static bool ValidateInjectedMethod(MethodDefinition method) {
            bool isNonInjectable =
                    !method.HasBody ||
                    method.MethodReturnType.ReturnType != method.Module.TypeSystem.Void ||
                    method.HasGenericParameters ||
                    method.HasParameters ||
                    !method.IsStatic
                ;

            return !isNonInjectable;
        }

        protected static bool ValidateInjecteeMethod(MethodDefinition method) {
            bool isNonInjectable =
                !method.HasBody ||
                // TODO: allow injection into methods with return value (issue #1)
                method.MethodReturnType.ReturnType != method.Module.TypeSystem.Void ||
                method.HasGenericParameters ||
                method.HasParameters
                ;

            return !isNonInjectable;
        }
    }
}