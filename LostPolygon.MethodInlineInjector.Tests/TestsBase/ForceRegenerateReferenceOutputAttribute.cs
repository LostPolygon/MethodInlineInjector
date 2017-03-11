using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class ForceRegenerateReferenceOutputAttribute :  NUnitAttribute, IApplyToContext {
        public bool Value { get; }

        public ForceRegenerateReferenceOutputAttribute()
            : this(true) {
        }

        public ForceRegenerateReferenceOutputAttribute(bool value) {
            Value = value;
        }

        public void ApplyToContext(TestExecutionContext context) {
            foreach (ITest test in context.CurrentTest.Tests) {
                test.Properties.Set(nameof(ForceRegenerateReferenceOutputAttribute).RemoveAttribute(), Value);
            }
        }
    }
}