using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Mono.Cecil;
using LostPolygon.Common.SimpleXmlSerialization;
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
                AssemblyDefinitionCachedData assemblyDefinitionCachedData = GetAssemblyDefinitionCachedData(sourceInjectedMethod.AssemblyPath);
                MethodDefinition[] matchingMethodDefinitions =
                    assemblyDefinitionCachedData
                    .AllMethods
                    .Where(methodDefinition => methodDefinition.GetFullSimpleName() == sourceInjectedMethod.MethodFullName)
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

        private AssemblyDefinitionCachedData GetAssemblyDefinitionCachedData(
            string assemblyPath,
            IEnumerable<ResolvedAllowedAssemblyReference> resolvedAllowedAssemblyReferences = null) {
            assemblyPath = Path.GetFullPath(assemblyPath);
            if (!_assemblyPathToAssemblyMap.TryGetValue(assemblyPath, out AssemblyDefinitionCachedData assemblyDefinitionData)) {
                Log.DebugFormat("Loading assembly at path '{0}'", assemblyPath);

                AssemblyDefinition assemblyDefinition = LoadAssemblyDefinition(assemblyPath, resolvedAllowedAssemblyReferences);
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

        private static AssemblyDefinition LoadAssemblyDefinition(
            string assemblyPath,
            IEnumerable<ResolvedAllowedAssemblyReference> resolvedAllowedAssemblyReferences
            ) {
            ReaderParameters parameters = new ReaderParameters();
            DefaultAssemblyResolver assemblyResolver = new IgnoringExceptionsAssemblyResolver();
            parameters.AssemblyResolver = assemblyResolver;
            parameters.ReadSymbols = false;

            if (resolvedAllowedAssemblyReferences != null) {
                assemblyResolver.ResolveFailure += (sender, reference) => {
                    foreach (ResolvedAllowedAssemblyReference resolvedAllowedAssemblyReference in resolvedAllowedAssemblyReferences) {
                        if (String.IsNullOrWhiteSpace(resolvedAllowedAssemblyReference.Path))
                            continue;

                        if (!reference.IsAssemblyReferencesMatch(
                            resolvedAllowedAssemblyReference.AssemblyNameReference,
                            resolvedAllowedAssemblyReference.StrictNameCheck))
                            continue;

                        Log.DebugFormat(
                            "Resolving referenced assembly {0} at path '{1}'",
                            resolvedAllowedAssemblyReference.AssemblyNameReference,
                            resolvedAllowedAssemblyReference.Path
                        );

                        AssemblyDefinition resolvedAssembly = AssemblyDefinition.ReadAssembly(resolvedAllowedAssemblyReference.Path);
                        return resolvedAssembly;
                    }
                    return null;
                };
            }
            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, parameters);
            return assemblyDefinition;
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
            List<IgnoredMemberReference> ignoredMemberReferences = new List<IgnoredMemberReference>();
            List<AllowedAssemblyReference> allowedAssemblyReferences = new List<AllowedAssemblyReference>();

            void LoadIgnoredMemberReferences(IEnumerable<IIgnoredMemberReference> items) {
                foreach (IIgnoredMemberReference item in items) {
                    if (item is IgnoredMemberReference ignoredMemberReference) {
                        ignoredMemberReferences.Add(ignoredMemberReference);
                        continue;
                    }

                    if (item is IgnoredMemberReferenceInclude ignoredMemberReferenceInclude) {
                        try {
                            Log.DebugFormat("Loading ignored member references list include at '{0}'", ignoredMemberReferenceInclude.Path);
                            string includeXml = File.ReadAllText(ignoredMemberReferenceInclude.Path);
                            IgnoredMemberReferencesIncludeLoader ignoredMemberReferencesIncludeLoader =
                                SimpleXmlSerializationUtility.XmlDeserializeFromString<IgnoredMemberReferencesIncludeLoader>(includeXml);
                            LoadIgnoredMemberReferences(ignoredMemberReferencesIncludeLoader.Items);
                        } catch (Exception e) {
                            Console.WriteLine(e);
                            throw new MethodInlineInjectorException(
                                $"Unable to load ignored member references list include at '{ignoredMemberReferenceInclude.Path}'",
                                e
                            );
                        }
                    }
                }
            }

            void LoadAllowedAssemblyReferences(IEnumerable<IAllowedAssemblyReference> items) {
                foreach (IAllowedAssemblyReference item in items) {
                    if (item is AllowedAssemblyReference allowedAssemblyReference) {
                        allowedAssemblyReferences.Add(allowedAssemblyReference);
                        continue;
                    }

                    if (item is AllowedAssemblyReferenceInclude allowedAssemblyReferenceInclude) {
                        Log.DebugFormat("Loading allowed assembly references list include at '{0}'", allowedAssemblyReferenceInclude.Path);
                        try {
                            string includeXml = File.ReadAllText(allowedAssemblyReferenceInclude.Path);
                            AllowedAssemblyReferenceIncludeLoader allowedAssemblyReferencesLoader =
                                SimpleXmlSerializationUtility.XmlDeserializeFromString<AllowedAssemblyReferenceIncludeLoader>(includeXml);
                            LoadAllowedAssemblyReferences(allowedAssemblyReferencesLoader.Items);
                        } catch (Exception e) {
                            throw new MethodInlineInjectorException(
                                $"Unable to load allowed assembly references list include at '{allowedAssemblyReferenceInclude.Path}'",
                                e
                            );
                        }
                    }
                }
            }

            LoadIgnoredMemberReferences(sourceInjecteeAssembly.IgnoredMemberReferences);
            LoadAllowedAssemblyReferences(sourceInjecteeAssembly.AllowedAssemblyReferences);

            List<ResolvedAllowedAssemblyReference> resolvedAllowedAssemblyReferences =
                allowedAssemblyReferences
                    .Select(item => new ResolvedAllowedAssemblyReference(AssemblyNameReference.Parse(item.Name), item.Path, item.StrictNameCheck))
                    .ToList();

            AssemblyDefinitionCachedData assemblyDefinitionCachedData =
                GetAssemblyDefinitionCachedData(sourceInjecteeAssembly.AssemblyPath, resolvedAllowedAssemblyReferences);

            Log.DebugFormat(
                "Calculating injectee methods in assembly '{0}'",
                assemblyDefinitionCachedData.AssemblyDefinition.MainModule.FullyQualifiedName);

            List<MethodDefinition> filteredInjecteeMethods =
                GetFilteredInjecteeMethods(assemblyDefinitionCachedData, ignoredMemberReferences);

            ResolvedInjecteeAssembly resolvedInjecteeAssembly =
                new ResolvedInjecteeAssembly(
                    assemblyDefinitionCachedData.AssemblyDefinition,
                    filteredInjecteeMethods,
                    resolvedAllowedAssemblyReferences
                );

            return resolvedInjecteeAssembly;
        }

        protected virtual List<MethodDefinition> GetFilteredInjecteeMethods(
            AssemblyDefinitionCachedData assemblyDefinitionCachedData,
            List<IgnoredMemberReference> ignoredMemberReferences) {
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

            Regex GetFilterRegex(IgnoredMemberReference ignoredMemberReference) {
                if (!regexCache.TryGetValue(ignoredMemberReference.Filter, out Regex filterRegex)) {
                    filterRegex = new Regex(ignoredMemberReference.Filter, RegexOptions.Compiled);
                    regexCache[ignoredMemberReference.Filter] = filterRegex;
                }

                return filterRegex;
            }

            bool TestString(IgnoredMemberReference ignoredMemberReference, string fullName) {
                if (ignoredMemberReference.IsRegex) {
                    if (GetFilterRegex(ignoredMemberReference).IsMatch(fullName))
                        return false;
                } else {
                    if (fullName.Contains(ignoredMemberReference.Filter))
                        return false;
                }

                return true;
            }

            bool TestType(TypeDefinition type) {
                foreach (IgnoredMemberReference ignoredMemberReference in ignoredMemberReferences) {
                    if (!ignoredMemberReference.FilterFlags.HasFlag(IgnoredMemberReferenceFlags.SkipTypes))
                        continue;

                    if (!TestTypeIgnored(type, ignoredMemberReference))
                        return false;
                }

                return true;
            }

            bool TestTypeIgnored(TypeDefinition type, IgnoredMemberReference ignoredMemberReference) {
                while (true) {
                    if (!TestString(ignoredMemberReference, type.FullName)) {
                        Log.DebugFormat("Ignored type '{0}'", type.FullName);
                        return false;
                    }

                    if (!ignoredMemberReference.MatchAncestors || type.BaseType == null)
                        break;

                    type = type.BaseType.GetDefinition();
                }
                return true;
            }

            IEnumerable<MethodDefinition> GetTestedMethodsFromType(TypeDefinition type) {
                HashSet<MethodDefinition> ignoredMethods = new HashSet<MethodDefinition>();

                foreach (PropertyDefinition property in type.Properties) {
                    foreach (IgnoredMemberReference ignoredMemberReference in ignoredMemberReferences) {
                        if (!ignoredMemberReference.FilterFlags.HasFlag(IgnoredMemberReferenceFlags.SkipProperties))
                            continue;

                        if (ProcessPropertyIgnored(property, ignoredMemberReference, ignoredMethods))
                            break;
                    }
                }

                foreach (MethodDefinition method in type.Methods) {
                    foreach (IgnoredMemberReference ignoredMemberReference in ignoredMemberReferences) {
                        if (!ignoredMemberReference.FilterFlags.HasFlag(IgnoredMemberReferenceFlags.SkipMethods))
                            continue;

                        if (ProcessMethodIgnored(method, ignoredMemberReference, ignoredMethods))
                            break;
                    }
                }

                return type.Methods.Except(ignoredMethods).Distinct();
            }

            bool ProcessPropertyIgnored(PropertyDefinition property, IgnoredMemberReference ignoredMemberReference, HashSet<MethodDefinition> ignoredMethods) {
                PropertyDefinition startProperty = property;
                while (true) {
                    if (!TestString(ignoredMemberReference, property.GetFullSimpleName())) {
                        Log.DebugFormat("Ignored property '{0}'", startProperty.GetFullSimpleName());
                        if (startProperty.GetMethod != null) {
                            ignoredMethods.Add(startProperty.GetMethod);
                        }
                        if (startProperty.SetMethod != null) {
                            ignoredMethods.Add(startProperty.SetMethod);
                        }
                        return false;
                    }

                    if (!ignoredMemberReference.MatchAncestors)
                        break;

                    PropertyDefinition baseProperty = property.GetBaseProperty();
                    if (baseProperty == property)
                        break;

                    property = baseProperty;
                }

                return true;
            }

            bool ProcessMethodIgnored(MethodDefinition method, IgnoredMemberReference ignoredMemberReference, HashSet<MethodDefinition> ignoredMethods) {
                MethodDefinition startMethod = method;
                while (true) {
                    if (!TestString(ignoredMemberReference, method.GetFullSimpleName())) {
                        ignoredMethods.Add(startMethod);
                        Log.DebugFormat("Ignored method '{0}'", startMethod.GetFullSimpleName());
                        return false;
                    }

                    if (!ignoredMemberReference.MatchAncestors)
                        break;

                    MethodDefinition baseMethod = method.GetBaseMethod();
                    if (baseMethod == method)
                        break;

                    method = baseMethod;
                }

                return true;
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
            return $"Injected method {method.GetFullSimpleName()} is not valid, reason: {message}";
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

        protected class IgnoringExceptionsAssemblyResolver : DefaultAssemblyResolver {
            public override AssemblyDefinition Resolve(AssemblyNameReference name) {
                try {
                    return base.Resolve(name);
                } catch {
                    Log.InfoFormat("Assembly reference {0} not resolved", name);
                    return null;
                }
            }
        }

        private abstract class FileIncludeLoader<TItem> where TItem : class {
            public IList<TItem> Items { get; } = new List<TItem>();

            [SerializationMethod]
            public static FileIncludeLoader<TItem> Serialize(FileIncludeLoader<TItem> instance, SimpleXmlSerializerBase serializer) {
                instance = instance ?? throw new ArgumentNullException(nameof(instance));

                // Skip root element when reading
                serializer.ProcessEnterChildOnRead();

                serializer.ProcessCollection(
                    instance.Items,
                    itemSerializer => serializer.CreateByKnownInheritors<TItem>(
                        serializer.CurrentXmlElement.Name,
                        itemSerializer
                    ));

                return instance;
            }
        }

        [XmlRoot("IgnoredMemberReferences")]
        private class IgnoredMemberReferencesIncludeLoader : FileIncludeLoader<IIgnoredMemberReference> {
            [SerializationMethod]
            public static IgnoredMemberReferencesIncludeLoader Serialize(
                IgnoredMemberReferencesIncludeLoader instance, SimpleXmlSerializerBase serializer
                ) {
                instance = instance ?? new IgnoredMemberReferencesIncludeLoader();
                FileIncludeLoader<IIgnoredMemberReference>.Serialize(instance, serializer);
                return instance;
            }
        }

        [XmlRoot("AllowedAssemblyReferences")]
        private class AllowedAssemblyReferenceIncludeLoader : FileIncludeLoader<IAllowedAssemblyReference> {
            [SerializationMethod]
            public static AllowedAssemblyReferenceIncludeLoader Serialize(
                AllowedAssemblyReferenceIncludeLoader instance, SimpleXmlSerializerBase serializer
                ) {
                instance = instance ?? new AllowedAssemblyReferenceIncludeLoader();
                FileIncludeLoader<IAllowedAssemblyReference>.Serialize(instance, serializer);
                return instance;
            }
        }
    }
}