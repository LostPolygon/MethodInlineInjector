using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Mono.Cecil;
using devtm.Cecil.Extensions;
using LostPolygon.MethodInlineInjector.Serialization;
using Mono.Collections.Generic;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectionConfigurationLoader {
        private readonly Dictionary<string, AssemblyDefinitionData> _assemblyPathToAssemblyDefinitionMap =
            new Dictionary<string, AssemblyDefinitionData>();
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
                    assemblyDefinitionData
                    .AllMethods
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
                ValidateInjectedMethod(matchedMethodDefinition);

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
            var memberReferenceBlacklistFilters = new List<InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter>();
            foreach (InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem memberReferenceBlacklistItem in sourceInjecteeAssembly.MemberReferenceBlacklist) {
                if (memberReferenceBlacklistItem is InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter memberReferenceBlacklistFilter) {
                    memberReferenceBlacklistFilters.Add(memberReferenceBlacklistFilter);
                    continue;
                }

                if (memberReferenceBlacklistItem is InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilterInclude memberReferenceBlacklistFilterInclude) {
                    string whiteListIncludeXml = File.ReadAllText(memberReferenceBlacklistFilterInclude.Path);
                    MemberReferenceBlacklistFilterInclude memberReferenceBlacklistFilterIncludedList =
                        SimpleXmlSerializationUtility.XmlDeserializeFromString<MemberReferenceBlacklistFilterInclude>(whiteListIncludeXml);
                    memberReferenceBlacklistFilters.AddRange(memberReferenceBlacklistFilterIncludedList.Items);
                }
            }

            var assemblyReferenceWhitelistFilters = new List<InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilter>();
            foreach (InjectionConfiguration.InjecteeAssembly.IAssemblyReferenceWhitelistItem assemblyReferenceWhitelistItem in sourceInjecteeAssembly.AssemblyReferenceWhitelist) {
                if (assemblyReferenceWhitelistItem is InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilter assemblyReferenceWhitelistFilter) {
                    assemblyReferenceWhitelistFilters.Add(assemblyReferenceWhitelistFilter);
                    continue;
                }

                if (assemblyReferenceWhitelistItem is InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilterInclude assemblyReferenceWhitelistFilterInclude) {
                    string whiteListIncludeXml = File.ReadAllText(assemblyReferenceWhitelistFilterInclude.Path);
                    AssemblyReferenceWhitelistFilterInclude assemblyReferenceWhitelistFilterIncludedList =
                        SimpleXmlSerializationUtility.XmlDeserializeFromString<AssemblyReferenceWhitelistFilterInclude>(whiteListIncludeXml);
                    assemblyReferenceWhitelistFilters.AddRange(assemblyReferenceWhitelistFilterIncludedList.Items);
                }
            }

            AssemblyDefinitionData assemblyDefinitionData = GetAssemblyDefinitionData(sourceInjecteeAssembly.AssemblyPath);

            List<MethodDefinition> filteredInjecteeMethods =
                GetFilteredInjecteeMethods(assemblyDefinitionData, memberReferenceBlacklistFilters);

            List<(AssemblyNameReference assemblyNameReference, bool isStrictCheck)> assemblyReferenceWhitelist =
                assemblyReferenceWhitelistFilters
                .Select(item => (AssemblyNameReference.Parse(item.Name), item.IsStrictNameCheck))
                .ToList();

            ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly =
                new ResolvedInjectionConfiguration.InjecteeAssembly(
                    sourceInjecteeAssembly,
                    assemblyDefinitionData,
                    filteredInjecteeMethods.AsReadOnly(),
                    assemblyReferenceWhitelist.AsReadOnly());

            return injecteeAssembly;
        }

        protected virtual List<MethodDefinition> GetFilteredInjecteeMethods(
            AssemblyDefinitionData assemblyDefinitionData,
            List<InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter> memberReferenceBlacklistFilters) {
            Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();

            TypeDefinition[] types =
                assemblyDefinitionData
                .AllTypes
                .Where(TestType)
                .ToArray();

            List<MethodDefinition> injecteeMethods =
                types
                .SelectMany(GetTestedMethodsFromType)
                .Where(ValidateInjecteeMethod)
                .ToList();

            return injecteeMethods;

            Regex GetFilterRegex(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter blacklistFilter) {
                Regex filterRegex;
                if (!regexCache.TryGetValue(blacklistFilter.Filter, out filterRegex)) {
                    filterRegex = new Regex(blacklistFilter.Filter, RegexOptions.Compiled);
                    regexCache[blacklistFilter.Filter] = filterRegex;
                }

                return filterRegex;
            }

            bool TestString(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter blacklistFilter, string fullName) {
                if (blacklistFilter.IsRegex) {
                    if (GetFilterRegex(blacklistFilter).IsMatch(fullName))
                        return false;
                } else {
                    if (fullName.Contains(blacklistFilter.Filter))
                        return false;
                }

                return true;
            }

            bool TestType(TypeDefinition type) {
                foreach (var blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterOptions.HasFlag(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes))
                        continue;

                    while (true) {
                        if (!TestString(blacklistFilter, type.FullName))
                            return false;

                        if (!blacklistFilter.FilterOptions.HasFlag(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.MatchAncestors) ||
                            type.BaseType == null)
                            break;

                        type = type.BaseType.GetDefinition();
                    }
                }

                return true;
            }

            IEnumerable<MethodDefinition> GetTestedMethodsFromType(TypeDefinition type) {
                HashSet<MethodDefinition> blacklistedMethods = new HashSet<MethodDefinition>();
                Collection<PropertyDefinition> properties = type.Properties;
                Collection<MethodDefinition> methods = type.Methods;

                // TODO: ancestor support
                foreach (var blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterOptions.HasFlag(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipProperties))
                        continue;

                    foreach (PropertyDefinition property in properties) {
                        if (TestString(blacklistFilter, property.FullName)) {
                            if (property.GetMethod != null) {
                                blacklistedMethods.Add(property.GetMethod);
                            }
                            if (property.SetMethod != null) {
                                blacklistedMethods.Add(property.SetMethod);
                            }
                        }
                    }
                }

                foreach (var blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterOptions.HasFlag(InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipMethods))
                        continue;

                    foreach (MethodDefinition method in methods) {
                        if (TestString(blacklistFilter, method.FullName)) {
                            blacklistedMethods.Add(method);
                        }
                    }
                }

                return
                    methods
                    .Except(blacklistedMethods);
            }
        }

        protected virtual void ValidateInjectedMethod(MethodDefinition injectedMethod) {
            if (!injectedMethod.HasBody)
                throw new MethodInlineInjectorException(CreateInvalidInjectedMethodMessage(injectedMethod, "method has no body"));

            if (injectedMethod.MethodReturnType.ReturnType != injectedMethod.Module.TypeSystem.Void)
                throw new MethodInlineInjectorException(CreateInvalidInjectedMethodMessage(injectedMethod, "injected method can't return any values"));

            if (injectedMethod.HasParameters)
                throw new MethodInlineInjectorException(CreateInvalidInjectedMethodMessage(injectedMethod, "injected method can't have parameters"));

            if (injectedMethod.HasGenericParameters)
                throw new MethodInlineInjectorException(CreateInvalidInjectedMethodMessage(injectedMethod, "injected method can't have generic parameters"));

            if (!injectedMethod.IsStatic)
                throw new MethodInlineInjectorException(CreateInvalidInjectedMethodMessage(injectedMethod, "injected method has to be static"));
        }

        protected virtual bool ValidateInjecteeMethod(MethodDefinition injecteeMethod) {
            bool isNonInjectable =
                !injecteeMethod.HasBody ||
                injecteeMethod.HasGenericParameters
                ;

            return !isNonInjectable;
        }

        protected static string CreateInvalidInjectedMethodMessage(MethodDefinition method, string message) {
            return $"Injected method {method.GetFullName()} is not valid, reason: {message}";
        }

        private class FileInclude<TItem> : SimpleXmlSerializable where TItem : class, ISimpleXmlSerializable {
            public List<TItem> Items { get; set; } = new List<TItem>();

            public override void Serialize() {
                base.Serialize();

                // Skip root element when reading
                SerializationHelper.ProcessAdvanceOnRead();

                this.ProcessCollection(Items);
            }
        }

        [XmlRoot("MemberReferenceBlacklist")]
        private class MemberReferenceBlacklistFilterInclude : FileInclude<InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter> {
        }

        [XmlRoot("AssemblyReferenceWhitelist")]
        private class AssemblyReferenceWhitelistFilterInclude : FileInclude<InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilter> {
        }
    }
}