using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class InjectorIntegrationTests {
        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodStart_ReturnFromSelf : InjectorIntegrationTestsBase {
            protected override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart;

            protected override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromSelf;
        }

        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodReturn_ReturnFromSelf : InjectorIntegrationTestsBase {
            protected override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn;

            protected override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromSelf;
        }

        [ApplyChildFixtureName]
        //[ForceRegenerateReferenceOutput]
        public class InjecteeMethodReturn_ReturnFromInjectee : InjectorIntegrationTestsBase {
            protected override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn;

            protected override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromInjectee;
        }


        [Explicit("not yet ready for CI")]
        [ApplyChildFixtureName]
        [ForceRegenerateReferenceOutput]
        public class InjecteeMethodStart_ReturnFromInjectee : InjectorIntegrationTestsBase {
            protected override InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition =>
                InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart;

            protected override InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour =>
                InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromInjectee;

            [Test]
            //[ValidReferenceOutput]
            public override void SingleStatementToSingleStatement() {
                base.SingleStatementToSingleStatement();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SingleStatementToComplex() {
                base.SingleStatementToComplex();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void TryCatchToComplex() {
                base.TryCatchToComplex();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void TryCatchToSingleStatement() {
                base.TryCatchToSingleStatement();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SingleStatementToReturnValue() {
                base.SingleStatementToReturnValue();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SingleStatementToCallResultReturnValue() {
                base.SingleStatementToCallResultReturnValue();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SimpleReturnToReturnValue() {
                base.SimpleReturnToReturnValue();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void DeepReturnToReturnValue() {
                base.DeepReturnToReturnValue();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SimpleReturnToWithParameters() {
                base.SimpleReturnToWithParameters();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void DeepReturnToWithParameters() {
                base.DeepReturnToWithParameters();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void ComplexToWithParameters() {
                base.ComplexToWithParameters();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SimpleReturnToWithRefParameter() {
                base.SimpleReturnToWithRefParameter();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void DeepReturnToWithRefParameter() {
                base.DeepReturnToWithRefParameter();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void ComplexToWithRefParameter() {
                base.ComplexToWithRefParameter();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void SimpleReturnToWithOutParameter() {
                base.SimpleReturnToWithOutParameter();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void DeepReturnToWithOutParameter() {
                base.DeepReturnToWithOutParameter();
            }

            [Test]
            //[ValidReferenceOutput]
            public override void ComplexToWithOutParameter() {
                base.ComplexToWithOutParameter();
            }
        }
    }
}
