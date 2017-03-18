using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class InjectorIntegrationTests {
        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodStart : InjectorIntegrationTestsBase {
            protected override MethodInjectionPosition MethodInjectionPosition =>
                MethodInjectionPosition.InjecteeMethodStart;
        }

        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodReturn : InjectorIntegrationTestsBase {
            protected override MethodInjectionPosition MethodInjectionPosition =>
                MethodInjectionPosition.InjecteeMethodReturn;
        }
    }
}
