using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjecteeAssembly {
        public AssemblyDefinition AssemblyDefinition { get; }
        public IReadOnlyList<MethodDefinition> InjecteeMethods { get; }
        public IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> AssemblyReferenceWhiteList { get; }

        public ResolvedInjecteeAssembly(
            AssemblyDefinition assemblyDefinition,
            IReadOnlyList<MethodDefinition> injecteeMethods,
            IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> assemblyReferenceWhiteList = null
        ) {
            AssemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));
            InjecteeMethods = injecteeMethods ?? throw new ArgumentNullException(nameof(assemblyReferenceWhiteList));
            AssemblyReferenceWhiteList =
                assemblyReferenceWhiteList ??
                ReadOnlyCollectionUtility<ResolvedAssemblyReferenceWhitelistItem>.Empty;
        }

        #region With.Fody

        public ResolvedInjecteeAssembly WithInjectedMethods(IReadOnlyList<MethodDefinition> value) => null;
        public ResolvedInjecteeAssembly WithAssemblyReferenceWhiteList(IReadOnlyList<ResolvedAssemblyReferenceWhitelistItem> value) => null;

        #endregion
    }
}