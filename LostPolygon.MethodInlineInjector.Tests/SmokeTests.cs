using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class SmokeTests {
        const string kInjectedLibraryName = "TestInjectedLibrary.dll";
        const string kInjecteeLibraryName = "TestInjecteeLibrary.dll";

        private readonly string[] kCommonFiles = {
            "TestData/Common/RuntimeAssembliesWhitelist.xml",
            "TestData/Common/UnityPerfomanceWhitelist.xml"
        };

        private TestEnvironmentConfig TestEnvironmentConfig { get; set; }

        [OneTimeSetUp]
        public void FixtureSetUp() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            TestEnvironmentConfig.SetTestEnvironmentConfigPath("TestEnvironmentConfig.ini");
            TestEnvironmentConfig = TestEnvironmentConfig.Instance;
            TestHelpers.DeployItems(kCommonFiles);
        }

        [OneTimeTearDown]
        public void FixtureTearDown() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.WorkDirectory);
        }

        [SetUp]
        public void SetUp() {
            string injectedLibraryPath = Path.Combine(
                TestEnvironmentConfig.SolutionDir, 
                "LostPolygon.MethodInlineInjector.Tests.TestInjectedLibrary",
                "bin",
                TestEnvironmentConfig.ConfigurationName,
                kInjectedLibraryName);

            File.Copy(injectedLibraryPath, kInjectedLibraryName, true);

            string injecteeLibraryPath = Path.Combine(
                TestEnvironmentConfig.SolutionDir, 
                "LostPolygon.MethodInlineInjector.Tests.TestInjecteeLibrary",
                "bin",
                TestEnvironmentConfig.ConfigurationName,
                kInjecteeLibraryName);

            File.Copy(injecteeLibraryPath, kInjecteeLibraryName, true);
        }

        [Test]
        public void Test1() {
            string injectionConfigPath = "TestData/TestInjection.xml";
            TestHelpers.DeployItems(injectionConfigPath);
            Program.Main(new[] { "TestInjection.xml" });
        }

        [Test]
        public void TestCecil() {
            File.Copy("Mono.Cecil.dll", kInjecteeLibraryName, true);
            string injectionConfigPath = "TestData/TestInjection.xml";
            TestHelpers.DeployItems(injectionConfigPath);
            Program.Main(new[] { "TestInjection.xml" });
        }
    }
}