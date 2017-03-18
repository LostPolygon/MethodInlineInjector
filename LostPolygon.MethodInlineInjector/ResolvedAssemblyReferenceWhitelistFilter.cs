using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedAssemblyReferenceWhitelistFilter {
        public AssemblyNameReference AssemblyNameReference { get; }
        public bool StrictNameCheck { get; }

        public ResolvedAssemblyReferenceWhitelistFilter(AssemblyNameReference assemblyNameReference, bool strictNameCheck = false) {
            AssemblyNameReference = assemblyNameReference ?? throw new ArgumentNullException(nameof(assemblyNameReference));
            StrictNameCheck = strictNameCheck;
        }

        #region With.Fody

        public ResolvedAssemblyReferenceWhitelistFilter WithIsStrictCheck(bool value) => null;

        #endregion
    }
}