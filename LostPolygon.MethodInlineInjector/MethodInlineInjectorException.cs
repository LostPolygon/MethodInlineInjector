using System;

namespace LostPolygon.MethodInlineInjector {
    [Serializable]
    internal class MethodInlineInjectorException : Exception {
        public MethodInlineInjectorException(string message) : base(message) {
        }
    }
}