using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedAssemblyReferenceWhitelistItem {
        public AssemblyNameReference AssemblyNameReference { get; }
        public bool StrictNameCheck { get; }

        public ResolvedAssemblyReferenceWhitelistItem(AssemblyNameReference assemblyNameReference, bool strictNameCheck = false) {
            AssemblyNameReference = assemblyNameReference ?? throw new ArgumentNullException(nameof(assemblyNameReference));
            StrictNameCheck = strictNameCheck;
        }

        #region With.Fody

        public ResolvedAssemblyReferenceWhitelistItem WithIsStrictCheck(bool value) => null;

        #endregion
    }
}