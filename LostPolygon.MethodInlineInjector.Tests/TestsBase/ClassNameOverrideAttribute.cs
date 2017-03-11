using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class ClassNameOverrideAttribute :  NUnitAttribute, IApplyToContext {
        public string Value { get; set; }

        public void ApplyToContext(TestExecutionContext context) {
            foreach (ITest test in context.CurrentTest.Tests) {
                test.Properties.Set(nameof(ClassNameOverrideAttribute).RemoveAttribute(), Value);
            }
        }
    }
}