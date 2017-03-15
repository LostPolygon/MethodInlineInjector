using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class ValidReferenceOutputAttribute : PropertyAttribute {
        public ValidReferenceOutputAttribute()
            : base(true) {
        }

        public ValidReferenceOutputAttribute(bool value) {
            Properties.Set(nameof(ValidReferenceOutputAttribute).RemoveAttribute(), value);
        }
    }
}