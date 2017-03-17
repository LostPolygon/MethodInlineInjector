using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Mono.Cecil;
using LostPolygon.MethodInlineInjector.Serialization;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectionConfigurationLoader {
        private readonly Dictionary<string, AssemblyDefinitionCachedData> _assemblyPathToAssemblyDefinitionMap =
            new Dictionary<string, AssemblyDefinitionCachedData>();
        private readonly InjectionConfiguration _injectionConfiguration;

        public static ResolvedInjectionConfiguration LoadFromInjectionConfiguration(InjectionConfiguration injectionConfiguration) {
            return new ResolvedInjectionConfigurationLoader(injectionConfiguration).Load();
        }

        protected ResolvedInjectionConfigurationLoader(InjectionConfiguration injectionConfiguration) {
            _injectionConfiguration = injectionConfiguration;
        }

        public ResolvedInjectionConfiguration Load() {
            Dictionary<AssemblyDefinition, List<ResolvedInjectedMethod>> injectedAssemblyToMethodsMap = GetInjectedMethods();

            List<ResolvedInjectedMethod> injectedMethods = new List<ResolvedInjectedMethod>();
            foreach (KeyValuePair<AssemblyDefinition, List<ResolvedInjectedMethod>> pair in injectedAssemblyToMethodsMap) {
                injectedMethods.AddRange(pair.Value);
            }

            List<ResolvedInjecteeAssembly> injecteeAssemblies = GetInjecteeAssemblies();

            Validate(injectedMethods, injecteeAssemblies);

            ResolvedInjectionConfiguration resolvedInjectionConfiguration =
                new ResolvedInjectionConfiguration(
                    injectedMethods,
                    injecteeAssemblies
                    );
            return resolvedInjectionConfiguration;
        }

        private void Validate(
            List<ResolvedInjectedMethod> injectedMethods,
            List<ResolvedInjecteeAssembly> injecteeAssemblies
            ) {
            AssemblyDefinition minInjecteeTargetRuntimeAssemblyDefinition = null;
            AssemblyDefinition maxInjectedTargetRuntimeAssemblyDefinition = null;
            foreach (ResolvedInjectedMethod injectedMethod in injectedMethods) {
                AssemblyDefinition assemblyDefinition = injectedMethod.MethodDefinition.Module.Assembly;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (maxInjectedTargetRuntimeAssemblyDefinition == null || targetRuntime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    maxInjectedTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            foreach (ResolvedInjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                AssemblyDefinition assemblyDefinition = injecteeAssembly.AssemblyDefinition;
                TargetRuntime targetRuntime = assemblyDefinition.MainModule.Runtime;
                if (minInjecteeTargetRuntimeAssemblyDefinition == null || targetRuntime > minInjecteeTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    minInjecteeTargetRuntimeAssemblyDefinition = assemblyDefinition;
                }
            }

            if (maxInjectedTargetRuntimeAssemblyDefinition == null)
                throw new InvalidOperationException(nameof(maxInjectedTargetRuntimeAssemblyDefinition) + " == null");

            foreach (ResolvedInjecteeAssembly injecteeAssembly in injecteeAssemblies) {
                if (injecteeAssembly.AssemblyDefinition.MainModule.Runtime < maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime) {
                    throw new MethodInlineInjectorException(
                        $"Injectee assembly '{injecteeAssembly.AssemblyDefinition}' " +
                        $"uses runtime version {injecteeAssembly.AssemblyDefinition.MainModule.Runtime}, " +
                        $"but assembly '{maxInjectedTargetRuntimeAssemblyDefinition}' used in injection uses runtime " +
                        $"version {maxInjectedTargetRuntimeAssemblyDefinition.MainModule.Runtime}."
                    );
                }
            }
        }

        private Dictionary<AssemblyDefinition, List<ResolvedInjectedMethod>> GetInjectedMethods() {
            var injectedAssemblyToMethodsMap = new Dictionary<AssemblyDefinition, List<ResolvedInjectedMethod>>();
            foreach (InjectedMethod sourceInjectedMethod in _injectionConfiguration.InjectedMethods) {
                AssemblyDefinitionCachedData assemblyDefinitionCachedData = GetAssemblyDefinitionData(sourceInjectedMethod.AssemblyPath);
                MethodDefinition[] matchingMethodDefinitions =
                    assemblyDefinitionCachedData
                    .AllMethods
                    .Where(methodDefinition => methodDefinition.GetFullName() == sourceInjectedMethod.MethodFullName)
                    .ToArray();

                if (matchingMethodDefinitions.Length == 0)
                    throw new MethodInlineInjectorException($"No matching methods found for {sourceInjectedMethod.MethodFullName}");

                if (matchingMethodDefinitions.Length > 2)
                    throw new MethodInlineInjectorException($"More than 1 matching method found for {sourceInjectedMethod.MethodFullName}");

                if (!injectedAssemblyToMethodsMap.TryGetValue(
                    assemblyDefinitionCachedData.AssemblyDefinition,
                    out List<ResolvedInjectedMethod> methodDefinitions)
                    ) {
                    methodDefinitions = new List<ResolvedInjectedMethod>();
                    injectedAssemblyToMethodsMap.Add(assemblyDefinitionCachedData.AssemblyDefinition, methodDefinitions);
                }

                MethodDefinition matchedMethodDefinition = matchingMethodDefinitions[0];
                ValidateInjectedMethod(matchedMethodDefinition);

                methodDefinitions.Add(
                    new ResolvedInjectedMethod(
                        matchedMethodDefinition,
                        sourceInjectedMethod.InjectionPosition,
                        sourceInjectedMethod.ReturnBehaviour
                    )
                );
            }

            return injectedAssemblyToMethodsMap;
        }

        private AssemblyDefinitionCachedData GetAssemblyDefinitionData(string assemblyPath) {
            assemblyPath = Path.GetFullPath(assemblyPath);
            if (!_assemblyPathToAssemblyDefinitionMap.TryGetValue(assemblyPath, out AssemblyDefinitionCachedData assemblyDefinitionData)) {
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
                assemblyDefinitionData = new AssemblyDefinitionCachedData(assemblyDefinition);
                _assemblyPathToAssemblyDefinitionMap.Add(assemblyPath, assemblyDefinitionData);
            }

            return assemblyDefinitionData;
        }

        private List<ResolvedInjecteeAssembly> GetInjecteeAssemblies() {
            var injecteeAssemblies = new List<ResolvedInjecteeAssembly>();
            foreach (InjecteeAssembly sourceInjecteeAssembly in _injectionConfiguration.InjecteeAssemblies) {
                ResolvedInjecteeAssembly resolvedInjecteeAssembly = GetInjecteeAssembly(sourceInjecteeAssembly);
                injecteeAssemblies.Add(resolvedInjecteeAssembly);
            }

            return injecteeAssemblies;
        }

        private ResolvedInjecteeAssembly GetInjecteeAssembly(InjecteeAssembly sourceInjecteeAssembly) {
            var memberReferenceBlacklistFilters = new List<MemberReferenceBlacklistFilter>();
            var assemblyReferenceWhitelistFilters = new List<AssemblyReferenceWhitelistFilter>();

            void LoadMemberReferenceBlacklistFilters(IEnumerable<IMemberReferenceBlacklistItem> items) {
                foreach (IMemberReferenceBlacklistItem memberReferenceBlacklistItem in items) {
                    if (memberReferenceBlacklistItem is MemberReferenceBlacklistFilter memberReferenceBlacklistFilter) {
                        memberReferenceBlacklistFilters.Add(memberReferenceBlacklistFilter);
                        continue;
                    }

                    if (memberReferenceBlacklistItem is LostPolygon.MethodInlineInjector.MemberReferenceBlacklistFilterInclude memberReferenceBlacklistFilterInclude) {
                        string whiteListIncludeXml = File.ReadAllText(memberReferenceBlacklistFilterInclude.Path);
                        MemberReferenceBlacklistFilterIncludeLoader memberReferenceBlacklistFilterIncludedList =
                            SimpleXmlSerializationUtility.XmlDeserializeFromString<MemberReferenceBlacklistFilterIncludeLoader>(whiteListIncludeXml);
                        LoadMemberReferenceBlacklistFilters(memberReferenceBlacklistFilterIncludedList.Items);
                    }
                }
            }

            void LoadAssemblyReferenceWhitelistFilters(IEnumerable<IAssemblyReferenceWhitelistItem> items) {
                foreach (IAssemblyReferenceWhitelistItem assemblyReferenceWhitelistItem in items) {
                    if (assemblyReferenceWhitelistItem is AssemblyReferenceWhitelistFilter assemblyReferenceWhitelistFilter) {
                        assemblyReferenceWhitelistFilters.Add(assemblyReferenceWhitelistFilter);
                        continue;
                    }

                    if (assemblyReferenceWhitelistItem is LostPolygon.MethodInlineInjector.AssemblyReferenceWhitelistFilterInclude assemblyReferenceWhitelistFilterInclude) {
                        string whiteListIncludeXml = File.ReadAllText(assemblyReferenceWhitelistFilterInclude.Path);
                        AssemblyReferenceWhitelistFilterIncludeLoader assemblyReferenceWhitelistFilterIncludedList =
                            SimpleXmlSerializationUtility.XmlDeserializeFromString<AssemblyReferenceWhitelistFilterIncludeLoader>(whiteListIncludeXml);
                        LoadAssemblyReferenceWhitelistFilters(assemblyReferenceWhitelistFilterIncludedList.Items);
                    }
                }
            }

            LoadMemberReferenceBlacklistFilters(sourceInjecteeAssembly.MemberReferenceBlacklist);
            LoadAssemblyReferenceWhitelistFilters(sourceInjecteeAssembly.AssemblyReferenceWhitelist);

            AssemblyDefinitionCachedData assemblyDefinitionCachedData = GetAssemblyDefinitionData(sourceInjecteeAssembly.AssemblyPath);

            List<MethodDefinition> filteredInjecteeMethods =
                GetFilteredInjecteeMethods(assemblyDefinitionCachedData, memberReferenceBlacklistFilters);

            List<ResolvedAssemblyReferenceWhitelistItem> assemblyReferenceWhitelist =
                assemblyReferenceWhitelistFilters
                .Select(item => new ResolvedAssemblyReferenceWhitelistItem(AssemblyNameReference.Parse(item.Name), item.StrictNameCheck))
                .ToList();

            ResolvedInjecteeAssembly resolvedInjecteeAssembly =
                new ResolvedInjecteeAssembly(
                    assemblyDefinitionCachedData.AssemblyDefinition,
                    filteredInjecteeMethods,
                    assemblyReferenceWhitelist);

            return resolvedInjecteeAssembly;
        }

        protected virtual List<MethodDefinition> GetFilteredInjecteeMethods(
            AssemblyDefinitionCachedData assemblyDefinitionCachedData,
            List<MemberReferenceBlacklistFilter> memberReferenceBlacklistFilters) {
            Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();

            TypeDefinition[] types =
                assemblyDefinitionCachedData
                .AllTypes
                .Where(TestType)
                .ToArray();

            List<MethodDefinition> injecteeMethods =
                types
                .SelectMany(GetTestedMethodsFromType)
                .Where(ValidateInjecteeMethod)
                .ToList();

            return injecteeMethods;

            Regex GetFilterRegex(MemberReferenceBlacklistFilter blacklistFilter) {
                if (!regexCache.TryGetValue(blacklistFilter.Filter, out Regex filterRegex)) {
                    filterRegex = new Regex(blacklistFilter.Filter, RegexOptions.Compiled);
                    regexCache[blacklistFilter.Filter] = filterRegex;
                }

                return filterRegex;
            }

            bool TestString(MemberReferenceBlacklistFilter blacklistFilter, string fullName) {
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
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipTypes))
                        continue;

                    while (true) {
                        if (!TestString(blacklistFilter, type.FullName))
                            return false;

                        if (!blacklistFilter.MatchAncestors || type.BaseType == null)
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
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipProperties))
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
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipMethods))
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

        protected class AssemblyDefinitionCachedData {
            public AssemblyDefinition AssemblyDefinition { get; }
            public IReadOnlyList<TypeDefinition> AllTypes { get; }
            public IReadOnlyList<MethodDefinition> AllMethods { get; }

            public AssemblyDefinitionCachedData(AssemblyDefinition assemblyDefinition) {
                AssemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));

                AllTypes = assemblyDefinition.MainModule.GetAllTypes().ToList();
                AllMethods = AllTypes.SelectMany(type => type.Methods).ToList();
            }
        }

        private class FileIncludeLoader<TItem> : SimpleXmlSerializable where TItem : class, ISimpleXmlSerializable {
            public IList<TItem> Items { get; } = new List<TItem>();

            protected override void Serialize() {
                base.Serialize();

                // Skip root element when reading
                SerializationHelper.ProcessAdvanceOnRead();

                this.ProcessCollection(
                    Items,
                    () => SimpleXmlSerializationHelper.CreateByKnownInheritors<TItem>(
                              SerializationHelper.XmlSerializationReader.Name
                          ));
            }
        }

        [XmlRoot("MemberReferenceBlacklist")]
        private class MemberReferenceBlacklistFilterIncludeLoader : FileIncludeLoader<IMemberReferenceBlacklistItem> {

        }

        [XmlRoot("AssemblyReferenceWhitelist")]
        private class AssemblyReferenceWhitelistFilterIncludeLoader : FileIncludeLoader<IAssemblyReferenceWhitelistItem> {

        }
    }
}