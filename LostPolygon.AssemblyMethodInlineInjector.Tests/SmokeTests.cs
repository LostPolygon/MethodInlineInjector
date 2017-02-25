using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace LostPolygon.AssemblyMethodInlineInjector.Tests {
    [TestFixture]
    public class SmokeTests {
        private readonly string[] kCommonFiles = {
            "TestData/Common/RuntimeAssembliesWhitelist.xml",
            "TestData/Common/UnityPerfomanceWhitelist.xml"
        };

        [SetUp]
        public void DeployCommonFiles() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            ItemDeployment.DeployItems(kCommonFiles);
        }

        [TearDown]
        public void TearDown() {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.WorkDirectory);
        }

        [Test]
        public void Test1() {
            string injectionConfigPath = "TestData/TestInjection.xml";
            ItemDeployment.DeployItems(injectionConfigPath);
            Program.Main(new[] { "TestInjection.xml" });
        }
    }
}