using System;
using System.IO;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedAllowedAssemblyReference {
        public AssemblyNameReference AssemblyNameReference { get; }
        public string Path { get; }
        public bool StrictNameCheck { get; }

        public ResolvedAllowedAssemblyReference(AssemblyNameReference assemblyNameReference, string path = null, bool strictNameCheck = false) {
            AssemblyNameReference = assemblyNameReference ?? throw new ArgumentNullException(nameof(assemblyNameReference));
            Path = path;
            StrictNameCheck = strictNameCheck;
        }

        #region With.Fody

        public ResolvedAllowedAssemblyReference WithIsStrictCheck(bool value) => null;

        #endregion

        public override string ToString() {
            string str = $"{nameof(AssemblyNameReference)}: '{AssemblyNameReference}', {nameof(StrictNameCheck)}: {StrictNameCheck}";
            if (!String.IsNullOrEmpty(Path)) {
                str = $"{str}, {nameof(Path)}: {Path}";
            }
            return str;
        }
    }
}