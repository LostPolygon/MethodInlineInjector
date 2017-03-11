using System;
using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public abstract class InjectorTestsBase : IntegrationTestMainBase {
        protected abstract InjectionConfiguration.InjectedMethod.MethodInjectionPosition MethodInjectionPosition { get; }
        protected abstract InjectionConfiguration.InjectedMethod.MethodReturnBehaviour MethodReturnBehaviour { get; }

        [Test]
        [ValidReferenceOutput]
        public void SingleStatementToSingleStatement() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void SingleStatementToComplex() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void TryCatchToComplex() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.TryCatch)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void TryCatchToSingleStatement() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.TryCatch)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void SingleStatementToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void SingleStatementToCallResultReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SingleStatement)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.CallResultReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void SimpleReturnToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void DeepReturnToReturnValue() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void SimpleReturnToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void DeepReturnToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void ComplexToWithParameters() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.ComplexMethod)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithParameters)}"
            );
        }

        //

        [Test]
        [ValidReferenceOutput]
        public void SimpleReturnToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void DeepReturnToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void ComplexToWithRefParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.ComplexMethod)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithRefParameter)}"
            );
        }

        //

        [Test]
        [ValidReferenceOutput]
        public void SimpleReturnToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void DeepReturnToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.DeepReturn)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        [Test]
        [ValidReferenceOutput]
        public void ComplexToWithOutParameter() {
            ExecuteSimpleTest(
                CreateInjectedMethod($"{InjectedClassName}.{nameof(TestInjectedMethods.ComplexMethod)}"),
                $"{InjecteeClassName}.{nameof(TestInjectee.WithOutParameter)}"
            );
        }

        protected InjectionConfiguration.InjectedMethod CreateInjectedMethod(string injectedMethodName) {
            return
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    injectedMethodName,
                    MethodInjectionPosition,
                    MethodReturnBehaviour
                );
        }
    }
}