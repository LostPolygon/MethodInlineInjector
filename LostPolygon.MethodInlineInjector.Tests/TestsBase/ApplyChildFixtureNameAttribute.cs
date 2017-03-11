using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal class ApplyChildFixtureNameAttribute :  NUnitAttribute, IApplyToContext {
        public void ApplyToContext(TestExecutionContext context) {
            string className = context.CurrentTest.FullName.Split('.').Last();

            foreach (ITest test in context.CurrentTest.Tests) {
                test.Properties.Set(nameof(ClassNameOverrideAttribute).RemoveAttribute(), className);
            }
        }
    }
}