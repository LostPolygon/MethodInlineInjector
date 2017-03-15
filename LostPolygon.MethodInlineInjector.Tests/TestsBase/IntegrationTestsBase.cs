using System;
using System.IO;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    public abstract class IntegrationTestsBase {
        public abstract string InjectedLibraryName { get; }
        public abstract string InjecteeLibraryName { get; }
        public abstract string InjectedClassName { get; }
        public abstract string InjecteeClassName { get; }
        public abstract string InjectedLibraryPath { get; }
        public abstract string InjecteeLibraryPath { get; }

        protected TestEnvironmentConfig TestEnvironmentConfig { get; set; }

        protected static ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectionConfiguration.InjectedMethod injectedMethod,
            string injecteeMethodName,
            bool assertFirstMethodMatch = true) {
            return ExecuteSimpleTest(new[] { injectedMethod }, new[] { injecteeMethodName }, assertFirstMethodMatch);
        }

        protected static ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectionConfiguration.InjectedMethod[] injectedMethods,
            string[] injecteeMethodNames,
            bool assertFirstMethodMatch = true) {
            InjectionConfiguration configuration = IntegrationTestsHelper.GetBasicInjectionConfiguration(injectedMethods);
            ResolvedInjectionConfiguration resolvedConfiguration = 
                IntegrationTestsHelper.GetBasicResolvedInjectionConfiguration(configuration, injecteeMethodNames);

            ExecuteSimpleTest(resolvedConfiguration, assertFirstMethodMatch);

            return resolvedConfiguration;
        }

        protected static void ExecuteSimpleTest(ResolvedInjectionConfiguration resolvedConfiguration, bool assertFirstMethodMatch = true) {
            IntegrationTestsHelper.ExecuteInjection(resolvedConfiguration);
            IntegrationTestsHelper.WriteModifiedAssembliesIfRequested(resolvedConfiguration);

            if (assertFirstMethodMatch) {
                bool validReferenceOutput = TestContext.CurrentContext.Test.Properties.Get(nameof(ValidReferenceOutputAttribute).RemoveAttribute()) is bool tmp1 && tmp1;
                bool forceRegenerateReferenceOutput = TestContext.CurrentContext.Test.Properties.Get(nameof(ForceRegenerateReferenceOutputAttribute).RemoveAttribute()) is bool tmp2 && tmp2;
                if (validReferenceOutput && !forceRegenerateReferenceOutput) {
                    IntegrationTestsHelper.AssertFirstMethod(resolvedConfiguration);
                } else {
                    IntegrationTestsHelper.WriteReferenceOutputFile(resolvedConfiguration);
                    Console.WriteLine(IntegrationTestsHelper.GetFormattedReferenceOutputFile());
                    Assert.Fail("Reference output not validated");
                }
            }
        }

        protected static void ExecuteSimpleInjection(InjectionConfiguration configuration) {
            ResolvedInjectionConfiguration resolvedConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(configuration);

            IntegrationTestsHelper.ExecuteInjection(resolvedConfiguration);
            IntegrationTestsHelper.WriteModifiedAssembliesIfRequested(resolvedConfiguration);
        }

        #region Setup

        [SetUp]
        public void SetUp() {
            TestContext.CurrentContext.Test.Properties.Set(nameof(InjectedLibraryName), InjectedLibraryName);
            TestContext.CurrentContext.Test.Properties.Set(nameof(InjecteeLibraryName), InjecteeLibraryName);
            TestContext.CurrentContext.Test.Properties.Set(nameof(InjectedClassName), InjectedClassName);

            string injectedLibraryPath = InjectedLibraryPath;
            File.Copy(injectedLibraryPath, InjectedLibraryName, true);

            string injecteeLibraryPath = InjecteeLibraryPath;
            File.Copy(injecteeLibraryPath, InjecteeLibraryName, true);
        }

        [OneTimeSetUp]
        public void FixtureSetUp() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            TestEnvironmentConfig.SetTestEnvironmentConfigPath("TestEnvironmentConfig.ini");
            TestEnvironmentConfig = TestEnvironmentConfig.Instance;
        }

        [OneTimeTearDown]
        public void FixtureTearDown() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.WorkDirectory);
        }

        #endregion
    }
}