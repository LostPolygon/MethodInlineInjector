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
            foreach (InjectedMethod sourceInjectedMethod in _injectionConfiguration.InjectedMethods) {
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
            foreach (InjecteeAssembly sourceInjecteeAssembly in _injectionConfiguration.InjecteeAssemblies) {
                ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly = GetInjecteeAssembly(sourceInjecteeAssembly);
                injecteeAssemblies.Add(injecteeAssembly);
            }

            return injecteeAssemblies;
        }

        private ResolvedInjectionConfiguration.InjecteeAssembly GetInjecteeAssembly(InjecteeAssembly sourceInjecteeAssembly) {
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
            List<MemberReferenceBlacklistFilter> memberReferenceBlacklistFilters) {
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

            Regex GetFilterRegex(MemberReferenceBlacklistFilter blacklistFilter) {
                Regex filterRegex;
                if (!regexCache.TryGetValue(blacklistFilter.Filter, out filterRegex)) {
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
                    if (!blacklistFilter.FilterOptions.HasFlag(MemberReferenceFilterFlags.SkipTypes))
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
                    if (!blacklistFilter.FilterOptions.HasFlag(MemberReferenceFilterFlags.SkipProperties))
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
                    if (!blacklistFilter.FilterOptions.HasFlag(MemberReferenceFilterFlags.SkipMethods))
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

        private class FileIncludeLoader<TItem> : SimpleXmlSerializable where TItem : class, ISimpleXmlSerializable {
            public List<TItem> Items { get; } = new List<TItem>();

            public override void Serialize() {
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