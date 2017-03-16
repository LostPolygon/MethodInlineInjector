using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedAssemblyReferenceWhitelistItem {
        public AssemblyNameReference AssemblyNameReference { get; }
        public bool IsStrictCheck { get; }

        public ResolvedAssemblyReferenceWhitelistItem(AssemblyNameReference assemblyNameReference, bool isStrictCheck = false) {
            AssemblyNameReference = assemblyNameReference ?? throw new ArgumentNullException(nameof(assemblyNameReference));
            IsStrictCheck = isStrictCheck;
        }

        #region With.Fody

        public ResolvedAssemblyReferenceWhitelistItem WithIsStrictCheck(bool value) => null;

        #endregion
    }
}