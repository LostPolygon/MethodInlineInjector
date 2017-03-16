using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjecteeAssembly {
        public InjecteeAssembly SourceInjecteeAssembly { get; }
        public AssemblyDefinitionData AssemblyDefinitionData { get; }
        public IReadOnlyList<MethodDefinition> InjecteeMethods { get; }
        public IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> AssemblyReferenceWhiteList { get; }

        public ResolvedInjecteeAssembly(
            InjecteeAssembly sourceInjecteeAssembly,
            AssemblyDefinitionData assemblyDefinitionData,
            IReadOnlyList<MethodDefinition> injecteeMethods,
            IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> assemblyReferenceWhiteList = null
        ) {
            SourceInjecteeAssembly = sourceInjecteeAssembly ?? throw new ArgumentNullException(nameof(sourceInjecteeAssembly));
            AssemblyDefinitionData = assemblyDefinitionData ?? throw new ArgumentNullException(nameof(assemblyDefinitionData));
            InjecteeMethods = injecteeMethods ?? throw new ArgumentNullException(nameof(assemblyReferenceWhiteList));
            AssemblyReferenceWhiteList =
                assemblyReferenceWhiteList ??
                ReadOnlyCollectionUtility<ResolvedAssemblyReferenceWhitelistItem>.Empty ;
        }

        #region With.Fody

        public ResolvedInjecteeAssembly WithInjectedMethods(IReadOnlyList<MethodDefinition> value) => null;
        public ResolvedInjecteeAssembly WithAssemblyReferenceWhiteList(IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> value) => null;

        #endregion
    }
}