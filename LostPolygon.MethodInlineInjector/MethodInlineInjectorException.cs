using System;

namespace LostPolygon.MethodInlineInjector {
    [Serializable]
    public class MethodInlineInjectorException : Exception {
        public MethodInlineInjectorException(string message) : base(message) {
        }
    }
}