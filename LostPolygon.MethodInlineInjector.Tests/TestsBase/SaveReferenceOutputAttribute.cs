using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class SaveReferenceOutputAttribute : PropertyAttribute {
        public SaveReferenceOutputAttribute() 
            : base(true) {
        }

        public SaveReferenceOutputAttribute(bool value) {
            Properties.Set("SaveReferenceOutput", value);
        }
    }
}