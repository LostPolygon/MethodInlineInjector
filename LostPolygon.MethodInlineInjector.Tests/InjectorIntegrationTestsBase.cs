using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public abstract class InjectorIntegrationTestsBase : IntegrationTestMainBase {
        protected abstract MethodInjectionPosition MethodInjectionPosition { get; }

        [Test]
        [ValidReferenceOutput]
        public virtual void SingleStatementToSingleStatement() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SingleStatementToComplex() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void TryCatchToComplex() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.TryCatch)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void TryCatchToSingleStatement() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.TryCatch)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SingleStatementToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SingleStatementToCallResultReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.CallResultReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SimpleReturnToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void DeepReturnToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SimpleReturnToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void DeepReturnToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void ComplexToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.Complex)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        //

        [Test]
        [ValidReferenceOutput]
        public virtual void SimpleReturnToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void DeepReturnToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void ComplexToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.Complex)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        //

        [Test]
        [ValidReferenceOutput]
        public virtual void SimpleReturnToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void DeepReturnToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void ComplexToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.Complex)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public virtual void SwitchToSingleStatement() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.Switch)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        protected InjectedMethod CreateInjectedMethod(string injectedMethodName) {
            return
                new InjectedMethod(
                    InjectedLibraryPath,
                    injectedMethodName,
                    MethodInjectionPosition
                );
        }
    }
}