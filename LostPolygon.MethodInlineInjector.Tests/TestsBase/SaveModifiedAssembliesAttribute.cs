using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class SaveModifiedAssembliesAttribute : PropertyAttribute {
        public SaveModifiedAssembliesAttribute()
            : base(true) {
        }

        public SaveModifiedAssembliesAttribute(bool value) {
            Properties.Set("SaveModifiedAssemblies", value);
        }
    }
}