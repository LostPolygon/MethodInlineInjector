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
using log4net;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectionConfigurationLoader {
        private static readonly ILog Log = LogManager.GetLogger(nameof(ResolvedInjectionConfigurationLoader));

        private readonly Dictionary<string, AssemblyDefinitionCachedData> _assemblyPathToAssemblyMap =
            new Dictionary<string, AssemblyDefinitionCachedData>();
        private readonly InjectionConfiguration _injectionConfiguration;

        public static ResolvedInjectionConfiguration LoadFromInjectionConfiguration(InjectionConfiguration injectionConfiguration) {
            return new ResolvedInjectionConfigurationLoader(injectionConfiguration).Load();
        }

        protected ResolvedInjectionConfigurationLoader(InjectionConfiguration injectionConfiguration) {
            _injectionConfiguration = injectionConfiguration;
        }

        public ResolvedInjectionConfiguration Load() {
            try {
                Log.Debug("Calculating filtered list of injected methods");
                Dictionary<AssemblyDefinition, List<ResolvedInjectedMethod>> injectedAssemblyToMethodsMap = GetInjectedMethods();

                List<ResolvedInjectedMethod> injectedMethods = new List<ResolvedInjectedMethod>();
                foreach (KeyValuePair<AssemblyDefinition, List<ResolvedInjectedMethod>> pair in injectedAssemblyToMethodsMap) {
                    injectedMethods.AddRange(pair.Value);
                }

                Log.Debug("Calculating list of injectee assemblies");
                List<ResolvedInjecteeAssembly> injecteeAssemblies = GetInjecteeAssemblies();
                Validate(injectedMethods, injecteeAssemblies);

                ResolvedInjectionConfiguration resolvedInjectionConfiguration =
                    new ResolvedInjectionConfiguration(
                        injectedMethods,
                        injecteeAssemblies
                    );
                return resolvedInjectionConfiguration;
            } catch (Exception e) {
                throw new MethodInlineInjectorException("Unknown exception", e);
            }
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
                    out List<ResolvedInjectedMethod> methodDefinitions
                    )) {
                    methodDefinitions = new List<ResolvedInjectedMethod>();
                    injectedAssemblyToMethodsMap.Add(assemblyDefinitionCachedData.AssemblyDefinition, methodDefinitions);
                }

                MethodDefinition matchedMethodDefinition = matchingMethodDefinitions[0];
                ValidateInjectedMethod(matchedMethodDefinition);

                methodDefinitions.Add(
                    new ResolvedInjectedMethod(
                        matchedMethodDefinition,
                        sourceInjectedMethod.InjectionPosition
                    )
                );
            }

            return injectedAssemblyToMethodsMap;
        }

        private AssemblyDefinitionCachedData GetAssemblyDefinitionData(string assemblyPath) {
            assemblyPath = Path.GetFullPath(assemblyPath);
            if (!_assemblyPathToAssemblyMap.TryGetValue(assemblyPath, out AssemblyDefinitionCachedData assemblyDefinitionData)) {
                Log.DebugFormat("Loading assembly at path '{0}'", assemblyPath);
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
                assemblyDefinitionData = new AssemblyDefinitionCachedData(assemblyDefinition);
                Log.DebugFormat(
                    "Loaded assembly at path '{0}': {1} types, {2} methods",
                    assemblyPath,
                    assemblyDefinitionData.AllTypes.Count,
                    assemblyDefinitionData.AllMethods.Count
                    );
                _assemblyPathToAssemblyMap.Add(assemblyPath, assemblyDefinitionData);
            }

            return assemblyDefinitionData;
        }

        private List<ResolvedInjecteeAssembly> GetInjecteeAssemblies() {
            List<ResolvedInjecteeAssembly> injecteeAssemblies = new List<ResolvedInjecteeAssembly>();
            foreach (InjecteeAssembly sourceInjecteeAssembly in _injectionConfiguration.InjecteeAssemblies) {
                ResolvedInjecteeAssembly resolvedInjecteeAssembly = GetInjecteeAssembly(sourceInjecteeAssembly);
                injecteeAssemblies.Add(resolvedInjecteeAssembly);
            }

            return injecteeAssemblies;
        }

        private ResolvedInjecteeAssembly GetInjecteeAssembly(InjecteeAssembly sourceInjecteeAssembly) {
            List<MemberReferenceBlacklistFilter> memberReferenceBlacklistFilters = new List<MemberReferenceBlacklistFilter>();
            List<AssemblyReferenceWhitelistFilter> assemblyReferenceWhitelistFilters = new List<AssemblyReferenceWhitelistFilter>();

            void LoadMemberReferenceBlacklistFilters(IEnumerable<IMemberReferenceBlacklistItem> items) {
                foreach (IMemberReferenceBlacklistItem memberReferenceBlacklistItem in items) {
                    if (memberReferenceBlacklistItem is MemberReferenceBlacklistFilter memberReferenceBlacklistFilter) {
                        memberReferenceBlacklistFilters.Add(memberReferenceBlacklistFilter);
                        continue;
                    }

                    if (memberReferenceBlacklistItem is MemberReferenceBlacklistFilterInclude memberReferenceBlacklistFilterInclude) {
                        try {
                            Log.DebugFormat("Loading member reference blacklist filter include at '{0}'", memberReferenceBlacklistFilterInclude.Path);
                            string whiteListIncludeXml = File.ReadAllText(memberReferenceBlacklistFilterInclude.Path);
                            MemberReferenceBlacklistFilterIncludeLoader memberReferenceBlacklistFilterIncludedList =
                                SimpleXmlSerializationUtility.XmlDeserializeFromString<MemberReferenceBlacklistFilterIncludeLoader>(whiteListIncludeXml);
                            LoadMemberReferenceBlacklistFilters(memberReferenceBlacklistFilterIncludedList.Items);
                        } catch (Exception e) {
                            Console.WriteLine(e);
                            throw new MethodInlineInjectorException(
                                $"Unable to load member reference blacklist filter include at '{memberReferenceBlacklistFilterInclude.Path}'",
                                e
                            );
                        }
                    }
                }
            }

            void LoadAssemblyReferenceWhitelistFilters(IEnumerable<IAssemblyReferenceWhitelistItem> items) {
                foreach (IAssemblyReferenceWhitelistItem assemblyReferenceWhitelistItem in items) {
                    if (assemblyReferenceWhitelistItem is AssemblyReferenceWhitelistFilter assemblyReferenceWhitelistFilter) {
                        assemblyReferenceWhitelistFilters.Add(assemblyReferenceWhitelistFilter);
                        continue;
                    }

                    if (assemblyReferenceWhitelistItem is AssemblyReferenceWhitelistFilterInclude assemblyReferenceWhitelistFilterInclude) {
                        Log.DebugFormat("Loading assembly reference whitelists filter include at '{0}'", assemblyReferenceWhitelistFilterInclude.Path);
                        try {
                            string whiteListIncludeXml = File.ReadAllText(assemblyReferenceWhitelistFilterInclude.Path);
                            AssemblyReferenceWhitelistFilterIncludeLoader assemblyReferenceWhitelistFilterIncludedList =
                                SimpleXmlSerializationUtility.XmlDeserializeFromString<AssemblyReferenceWhitelistFilterIncludeLoader>(whiteListIncludeXml);
                            LoadAssemblyReferenceWhitelistFilters(assemblyReferenceWhitelistFilterIncludedList.Items);
                        } catch (Exception e) {
                            throw new MethodInlineInjectorException(
                                $"Unable to load assembly reference whitelist filter include at '{assemblyReferenceWhitelistFilterInclude.Path}'",
                                e
                            );
                        }
                    }
                }
            }

            LoadMemberReferenceBlacklistFilters(sourceInjecteeAssembly.MemberReferenceBlacklist);
            LoadAssemblyReferenceWhitelistFilters(sourceInjecteeAssembly.AssemblyReferenceWhitelist);

            AssemblyDefinitionCachedData assemblyDefinitionCachedData = GetAssemblyDefinitionData(sourceInjecteeAssembly.AssemblyPath);

            Log.DebugFormat(
                "Calculating injectee methods in assembly '{0}'",
                assemblyDefinitionCachedData.AssemblyDefinition.MainModule.FullyQualifiedName);

            List<MethodDefinition> filteredInjecteeMethods =
                GetFilteredInjecteeMethods(assemblyDefinitionCachedData, memberReferenceBlacklistFilters);

            List<ResolvedAssemblyReferenceWhitelistFilter> assemblyReferenceWhitelist =
                assemblyReferenceWhitelistFilters
                .Select(item => new ResolvedAssemblyReferenceWhitelistFilter(AssemblyNameReference.Parse(item.Name), item.StrictNameCheck))
                .ToList();

            ResolvedInjecteeAssembly resolvedInjecteeAssembly =
                new ResolvedInjecteeAssembly(
                    assemblyDefinitionCachedData.AssemblyDefinition,
                    filteredInjecteeMethods,
                    assemblyReferenceWhitelist
                );

            return resolvedInjecteeAssembly;
        }

        protected virtual List<MethodDefinition> GetFilteredInjecteeMethods(
            AssemblyDefinitionCachedData assemblyDefinitionCachedData,
            List<MemberReferenceBlacklistFilter> memberReferenceBlacklistFilters) {
            Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();

            Log.DebugFormat("Number of types before filtering: {0}", assemblyDefinitionCachedData.AllTypes.Count);
            TypeDefinition[] types =
                assemblyDefinitionCachedData
                .AllTypes
                .Where(TestType)
                .ToArray();
            Log.DebugFormat("Number of types after filtering: {0}", types.Length);

            List<MethodDefinition> injecteeMethods =
                types
                .SelectMany(GetTestedMethodsFromType)
                .Where(ValidateInjecteeMethod)
                .ToList();
            Log.DebugFormat("Number of methods after filtering: {0}", injecteeMethods.Count);

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
                foreach (MemberReferenceBlacklistFilter blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipTypes))
                        continue;

                    while (true) {
                        if (!TestString(blacklistFilter, type.FullName)) {
                            Log.DebugFormat("Blacklisted type '{0}'", type.FullName);
                            return false;
                        }

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
                foreach (MemberReferenceBlacklistFilter blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipProperties))
                        continue;

                    foreach (PropertyDefinition property in properties) {
                        if (!TestString(blacklistFilter, property.FullName)) {
                            Log.DebugFormat("Blacklisted property '{0}.{1}'", property.DeclaringType.FullName, property.FullName);
                            continue;
                        }

                        if (property.GetMethod != null) {
                            blacklistedMethods.Add(property.GetMethod);
                        }
                        if (property.SetMethod != null) {
                            blacklistedMethods.Add(property.SetMethod);
                        }
                    }
                }

                foreach (MemberReferenceBlacklistFilter blacklistFilter in memberReferenceBlacklistFilters) {
                    if (!blacklistFilter.FilterFlags.HasFlag(MemberReferenceBlacklistFilterFlags.SkipMethods))
                        continue;

                    foreach (MethodDefinition method in methods) {
                        string methodFullName = method.GetFullName();
                        if (!TestString(blacklistFilter, methodFullName)) {
                            Log.DebugFormat("Blacklisted method '{0}'", methodFullName);
                            continue;
                        }

                        blacklistedMethods.Add(method);
                    }
                }

                return methods.Except(blacklistedMethods);
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
                injecteeMethod.HasGenericParameters;

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

        private abstract class FileIncludeLoader<TItem> where TItem : class {
            public IList<TItem> Items { get; } = new List<TItem>();

            [SerializationMethod]
            public static FileIncludeLoader<TItem> Serialize(FileIncludeLoader<TItem> instance, SimpleXmlSerializerBase serializer) {
                instance = instance ?? throw new ArgumentNullException(nameof(instance));

                // Skip root element when reading
                serializer.ProcessAdvanceOnRead();

                serializer.ProcessCollection(
                    instance.Items,
                    itemSerializer => serializer.CreateByKnownInheritors<TItem>(
                        serializer.CurrentXmlElement.Name,
                        itemSerializer
                    ));

                return instance;
            }
        }

        [XmlRoot("MemberReferenceBlacklist")]
        private class MemberReferenceBlacklistFilterIncludeLoader : FileIncludeLoader<IMemberReferenceBlacklistItem> {
            [SerializationMethod]
            public static MemberReferenceBlacklistFilterIncludeLoader Serialize(
                MemberReferenceBlacklistFilterIncludeLoader instance, SimpleXmlSerializerBase serializer
                ) {
                instance = instance ?? new MemberReferenceBlacklistFilterIncludeLoader();
                FileIncludeLoader<IMemberReferenceBlacklistItem>.Serialize(instance, serializer);
                return instance;
            }
        }

        [XmlRoot("AssemblyReferenceWhitelist")]
        private class AssemblyReferenceWhitelistFilterIncludeLoader : FileIncludeLoader<IAssemblyReferenceWhitelistItem> {
            [SerializationMethod]
            public static AssemblyReferenceWhitelistFilterIncludeLoader Serialize(
                AssemblyReferenceWhitelistFilterIncludeLoader instance, SimpleXmlSerializerBase serializer
                ) {
                instance = instance ?? new AssemblyReferenceWhitelistFilterIncludeLoader();
                FileIncludeLoader<IAssemblyReferenceWhitelistItem>.Serialize(instance, serializer);
                return instance;
            }
        }
    }
}