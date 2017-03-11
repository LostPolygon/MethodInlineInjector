using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class InjectorTests {
        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodStart_ReturnFromSelf : InjectorTestsBase {
            public override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart;

            public override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromSelf;
        }

        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodReturn_ReturnFromSelf : InjectorTestsBase {
            public override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn;

            public override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromSelf;
        }

        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodReturn_ReturnFromInjectee : InjectorTestsBase {
            public override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn;

            public override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromInjectee;
        }
    }
}
