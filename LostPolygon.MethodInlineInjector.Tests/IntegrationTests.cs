using System;
using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class IntegrationTests : IntegrationTestsBase {
        [Test]
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
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void NonStatic() {
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
        public void FieldDependent() {
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
        public void TypeDependent() {
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