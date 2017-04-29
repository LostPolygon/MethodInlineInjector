using System;
using System.Globalization;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    public abstract class IntegrationTestsBase : TestEnvironmentTestsBase {
        public abstract string InjectedLibraryName { get; }
        public abstract string InjecteeLibraryName { get; }
        public abstract string InjectedClassName { get; }
        public abstract string InjecteeClassName { get; }
        public abstract string InjectedLibraryPath { get; }
        public abstract string InjecteeLibraryPath { get; }

        protected static ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectedMethod injectedMethod,
            string injecteeMethodName,
            bool assertFirstMethodMatch = true) {
            return ExecuteSimpleTest(new[] { injectedMethod }, new[] { injecteeMethodName }, assertFirstMethodMatch);
        }

        protected static ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectionConfiguration configuration,
            string[] injecteeMethodNames,
            bool assertFirstMethodMatch = true) {
            ResolvedInjectionConfiguration resolvedConfiguration =
                IntegrationTestsHelper.GetBasicResolvedInjectionConfiguration(configuration, injecteeMethodNames);

            ExecuteSimpleTest(resolvedConfiguration, assertFirstMethodMatch);

            return resolvedConfiguration;
        }

        protected static ResolvedInjectionConfiguration ExecuteSimpleTest(
            InjectedMethod[] injectedMethods,
            string[] injecteeMethodNames,
            bool assertFirstMethodMatch = true) {
            InjectionConfiguration configuration = 
                IntegrationTestsHelper.GetBasicInjectionConfiguration(true, true, injectedMethods);
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            TestContext.CurrentContext.Test.Properties.Set(nameof(InjectedLibraryName), InjectedLibraryName);
            TestContext.CurrentContext.Test.Properties.Set(nameof(InjecteeLibraryName), InjecteeLibraryName);
            TestContext.CurrentContext.Test.Properties.Set(nameof(InjectedClassName), InjectedClassName);

            void CopyIfDateMismatch(string source, string destination) {
                if (File.GetLastWriteTimeUtc(source) == File.GetLastWriteTimeUtc(destination))
                    return;

                File.Copy(source, destination, true);
            }

            CopyIfDateMismatch(InjectedLibraryPath, InjectedLibraryName);
            CopyIfDateMismatch(InjecteeLibraryPath, InjecteeLibraryName);
        }

        #endregion
    }
}