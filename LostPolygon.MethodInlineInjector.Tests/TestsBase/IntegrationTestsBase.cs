using System;
using System.IO;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    public abstract class IntegrationTestsBase {
        public abstract string InjectedLibraryName { get; }
        public abstract string InjecteeLibraryName { get; }
        public abstract string InjectedClassName { get; }
        public abstract string InjectedLibraryPath { get; }
        public abstract string InjecteeLibraryPath { get; }

        protected abstract string[] CommonFiles { get; }

        protected TestEnvironmentConfig TestEnvironmentConfig { get; set; }

        protected ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectionConfiguration.InjectedMethod injectedMethod,
            string injecteeMethodName) {
            return ExecuteSimpleTest(new[] { injectedMethod }, new[] { injecteeMethodName });
        }

        protected ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectionConfiguration.InjectedMethod[] injectedMethods,
            string[] injecteeMethodNames) {
            InjectionConfiguration configuration = IntegrationTestsHelper.GetBasicInjectionConfiguration(injectedMethods);
            ResolvedInjectionConfiguration resolvedConfiguration = IntegrationTestsHelper.GetBasicResolvedInjectionConfiguration(configuration, injecteeMethodNames);
            IntegrationTestsHelper.ExecuteInjection(resolvedConfiguration);

            if (TestContext.CurrentContext.Test.Properties.Get("SaveModifiedAssemblies") is bool saveModifiedAssemblies && saveModifiedAssemblies) {
                foreach (ResolvedInjectionConfiguration.InjecteeAssembly injecteeAssembly in resolvedConfiguration.InjecteeAssemblies) {
                    injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.Write(injecteeAssembly.SourceInjecteeAssembly.AssemblyPath);
                }
            }

            if (TestContext.CurrentContext.Test.Properties.Get("SaveReferenceOutput") is bool saveReferenceOutput && saveReferenceOutput) {
                IntegrationTestsHelper.WriteReferenceOutputFile(resolvedConfiguration);
                Console.WriteLine(IntegrationTestsHelper.GetFormattedReferenceOutputFile());
            } else {
                IntegrationTestsHelper.AssertFirstMethod(resolvedConfiguration);
            }

            return resolvedConfiguration;
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
            ItemDeployment.DeployItems(CommonFiles);
        }

        [OneTimeTearDown]
        public void FixtureTearDown() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.WorkDirectory);
        }

        #endregion
    }
}