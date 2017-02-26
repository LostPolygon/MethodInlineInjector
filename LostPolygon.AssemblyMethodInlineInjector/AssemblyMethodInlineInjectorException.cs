using System;

namespace LostPolygon.AssemblyMethodInlineInjector {
    [Serializable]
    internal class AssemblyMethodInlineInjectorException : Exception {
        public AssemblyMethodInlineInjectorException(string message) : base(message) {
        }
    }
}