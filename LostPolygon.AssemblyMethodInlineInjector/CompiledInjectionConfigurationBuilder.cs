using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using devtm.Cecil.Extensions;
using LostPolygon.AssemblyMethodInlineInjector.Configuration;
using Mono.Cecil;
using UnityEngine;

namespace LostPolygon.AssemblyMethodInlineInjector {
    internal class CompiledInjectionConfigurationBuilder {
        private readonly Dictionary<string, AssemblyDefinitionData> _assemblyPathToAssemblyDefinitionMap = new Dictionary<string, AssemblyDefinitionData>();
        private readonly InjectionConfiguration _injectionConfiguration;

        public CompiledInjectionConfigurationBuilder(InjectionConfiguration injectionConfiguration) {
            _injectionConfiguration = injectionConfiguration;
        }

        public CompiledInjectionConfiguration Build() {
            Dictionary<AssemblyDefinition, List<CompiledInjectionConfiguration.InjectedMethod>> injectedAssemblyToMethodsMap = GetInjectedMethods();
            
            List<CompiledInjectionConfiguration.InjectedAssemblyMethods> injectedAssemblyMethods = new List<CompiledInjectionConfiguration.InjectedAssemblyMethods>();
            foreach (KeyValuePair<AssemblyDefinition, List<CompiledInjectionConfiguration.InjectedMethod>> pair in injectedAssemblyToMethodsMap) {
                injectedAssemblyMethods.Add(new CompiledInjectionConfiguration.InjectedAssemblyMethods(pair.Key, pair.Value.AsReadOnly()));
            }

            List<CompiledInjectionConfiguration.InjecteeAssembly> injecteeAssemblies = GetInjecteeAssemblies();

            Validate(injectedAssemblyMethods, injecteeAssemblies);

            CompiledInjectionConfiguration compiledInjectionConfiguration = 
                new CompiledInjectionConfiguration(
                    injectedAssemblyMethods.AsReadOnly(), 
                    injecteeAssemblies.AsReadOnly()
                    );
            return compiledInjectionConfiguration;
        }

        private void Validate(
            List<CompiledInjectionConfiguration.InjectedAssemblyMethods> injectedAssemblyMethods, 
            List<CompiledInjectionConfiguration.InjecteeAssembly> injecteeAssemblies
            ) {
            AssemblyDefinition minInjecteeTargetRuntimeAssemblyDefinition = null;
            AssemblyDefinition maxInjectedTargetRuntimeAssemblyDefinition = null;
            foreach (CompiledInjectionConfiguration.InjectedAssemblyMethods injectedAssembly in injectedAssemblyMethods) {
                AssemblyDefinition assemblyDefinition = injectedAssembly.AssemblyDefinition;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (maxInjectedTargetRuntimeAssemblyDefinition == null || targetRuntime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    maxInjectedTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            foreach (CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                AssemblyDefinition assemblyDefinition = injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (minInjecteeTargetRuntimeAssemblyDefinition == null || targetRuntime > minInjecteeTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    minInjecteeTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            foreach (CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                if (injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.MainModule.Runtime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    throw new AssemblyMethodInlineInjectorException(
                        $"Injectee assembly '{injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition}' " +
                        $"uses runtime version {injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.MainModule.Runtime}, " +
                        $"but assembly '{maxInjectedTargetRuntimeAssemblyDefinition}' used in injection uses runtime version {maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime}."
                        );        
                }
            }
        }

        private Dictionary<AssemblyDefinition, List<CompiledInjectionConfiguration.InjectedMethod>> GetInjectedMethods() {
            var injectedAssemblyToMethodsMap = new Dictionary<AssemblyDefinition, List<CompiledInjectionConfiguration.InjectedMethod>>();
            foreach (InjectionConfiguration.InjectedMethod sourceInjectedMethod in _injectionConfiguration.InjectedMethods) {
                AssemblyDefinitionData assemblyDefinitionData = GetAssemblyDefinitionData(sourceInjectedMethod.AssemblyPath);
                MethodDefinition[] matchingMethodDefinitions =
                    assemblyDefinitionData.AllMethods
                        .Where(methodDefinition => methodDefinition.GetFullName() == sourceInjectedMethod.MethodFullName)
                        .ToArray();

                if (matchingMethodDefinitions.Length == 0)
                    throw new AssemblyMethodInlineInjectorException($"No matching methods found for {sourceInjectedMethod.MethodFullName}");

                if (matchingMethodDefinitions.Length > 2)
                    throw new AssemblyMethodInlineInjectorException($"More than 1 matching method found for {sourceInjectedMethod.MethodFullName}");

                List<CompiledInjectionConfiguration.InjectedMethod> methodDefinitions;
                if (!injectedAssemblyToMethodsMap.TryGetValue(assemblyDefinitionData.AssemblyDefinition, out methodDefinitions)) {
                    methodDefinitions = new List<CompiledInjectionConfiguration.InjectedMethod>();
                    injectedAssemblyToMethodsMap.Add(assemblyDefinitionData.AssemblyDefinition, methodDefinitions);
                }

                MethodDefinition matchedMethodDefinition = matchingMethodDefinitions[0];
                methodDefinitions.Add(new CompiledInjectionConfiguration.InjectedMethod(sourceInjectedMethod, matchedMethodDefinition));
            }

            return injectedAssemblyToMethodsMap;
        }

        private AssemblyDefinitionData GetAssemblyDefinitionData(string assemblyPath) {
            assemblyPath = Path.GetFullPath(assemblyPath);
            AssemblyDefinitionData assemblyDefinitionData;
            if (!_assemblyPathToAssemblyDefinitionMap.TryGetValue(assemblyPath, out assemblyDefinitionData)) {
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
                assemblyDefinitionData = new AssemblyDefinitionData(assemblyDefinition);
                _assemblyPathToAssemblyDefinitionMap.Add(assemblyPath, assemblyDefinitionData);
            }

            return assemblyDefinitionData;
        }

        private List<CompiledInjectionConfiguration.InjecteeAssembly> GetInjecteeAssemblies() {
            var injecteeAssemblies = new List<CompiledInjectionConfiguration.InjecteeAssembly>();
            foreach (InjectionConfiguration.InjecteeAssembly sourceInjecteeAssembly in _injectionConfiguration.InjecteeAssemblies) {
                CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly = GetInjecteeAssembly(sourceInjecteeAssembly);
                injecteeAssemblies.Add(injecteeAssembly);
            }

            return injecteeAssemblies;
        }

        private CompiledInjectionConfiguration.InjecteeAssembly GetInjecteeAssembly(InjectionConfiguration.InjecteeAssembly sourceInjecteeAssembly) {
            var memberReferenceWhitelistFilters = new List<InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter>();
            foreach (InjectionConfiguration.InjecteeAssembly.IMemberReferenceWhitelistItem memberReferenceWhitelistItem in sourceInjecteeAssembly.MemberReferenceWhitelist) {
                var memberReferenceWhitelistFilter = memberReferenceWhitelistItem as InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter;
                if (memberReferenceWhitelistFilter != null) {
                    memberReferenceWhitelistFilters.Add(memberReferenceWhitelistFilter);
                    continue;
                }

                var memberReferenceWhitelistFilterInclude = memberReferenceWhitelistItem as InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilterInclude;
                if (memberReferenceWhitelistFilterInclude != null) {
                    string whiteListIncludeXml = File.ReadAllText(memberReferenceWhitelistFilterInclude.Path);
                    MemberReferenceWhitelistFilterInclude memberReferenceWhitelistFilterIncludedList = XmlSerializationUtility.XmlDeserializeFromString<MemberReferenceWhitelistFilterInclude>(whiteListIncludeXml);
                    memberReferenceWhitelistFilters.AddRange(memberReferenceWhitelistFilterIncludedList.MemberReferenceWhitelist);
                }
            }

            AssemblyDefinitionData assemblyDefinitionData = GetAssemblyDefinitionData(sourceInjecteeAssembly.AssemblyPath);
            List<MethodDefinition> injecteeMethodDefinitions = new List<MethodDefinition>();
            injecteeMethodDefinitions.AddRange(assemblyDefinitionData.AllMethods);

            CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly = 
                new CompiledInjectionConfiguration.InjecteeAssembly(sourceInjecteeAssembly, assemblyDefinitionData, injecteeMethodDefinitions);

            return injecteeAssembly;
        }
    }
}