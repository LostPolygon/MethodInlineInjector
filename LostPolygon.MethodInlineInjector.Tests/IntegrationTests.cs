using System;
using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase {
        private const bool kDefaultSaveReferenceOutput = false
#if TRUE1
            || true
#endif
            ;

        [Test]
        [SaveReferenceOutput(kDefaultSaveReferenceOutput)]
        public void SingleStatementToSingleStatement() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    IntegrationTestsHelper.GetInjectedMethodFullName(nameof(TestInjectedMethods.SingleStatement))
                ),
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.Complex)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.Complex)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.Complex)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.Complex)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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
                $"{nameof(TestInjecteeLibrary)}.{nameof(TestInjectee)}.{nameof(TestInjectee.SingleStatement)}"
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