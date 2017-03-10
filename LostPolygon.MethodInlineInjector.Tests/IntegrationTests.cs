using System;
using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase {
        private const bool kDefaultSaveReferenceOutput = false
#if TRUE
            || true
#endif
            ;

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        [SaveModifiedAssemblies]
        public void SingleStatementToSingleStatement() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToSingleStatementAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToComplex() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToComplexAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void TryCatchToComplex() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.TryCatch))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void TryCatchToComplexAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.TryCatch)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.Complex)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void TryCatchToSingleStatement() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.TryCatch))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void TryCatchToSingleStatementAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.TryCatch)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }


        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToReturnValue() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToReturnValueAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToCallResultReturnValue() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.CallResultReturnValue)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToCallResultReturnValueAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.CallResultReturnValue)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SimpleReturnToReturnValue() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SimpleReturn))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SimpleReturnToReturnValueAtReturn() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SimpleReturn)),
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.ReturnValue)}"
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void Failing_NonStaticInjected() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.NonStatic))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void Failing_FieldDependentInjected() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.FieldDependent))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void Failing_TypeDependentInjected() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.TypeDependent))
                ),
                $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        #region Setup

        public override string InjectedLibraryName => "TestInjectedLibrary.dll";
        public override string InjecteeLibraryName => "TestInjecteeLibrary.dll";
        public override string InjectedLibraryPath =>
            Path.Combine(
                TestEnvironmentConfig.SolutionDir,
                "LostPolygon.MethodInlineInjector.Tests.TestInjectedLibrary",
                "bin",
                TestEnvironmentConfig.ConfigurationName,
                InjectedLibraryName);

        public override string InjecteeLibraryPath =>
            Path.Combine(
                TestEnvironmentConfig.SolutionDir,
                "LostPolygon.MethodInlineInjector.Tests.TestInjecteeLibrary",
                "bin",
                TestEnvironmentConfig.ConfigurationName,
                InjecteeLibraryName);
        public override string InjectedClassName => "TestInjectedLibrary.TestInjectedMethods";

        protected override string[] CommonFiles => new[] {
            "TestData/Common/RuntimeAssembliesWhitelist.xml",
            "TestData/Common/UnityPerfomanceWhitelist.xml"
        };

        #endregion
    }
}