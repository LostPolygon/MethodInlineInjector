using System.IO;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    public abstract class TestEnvironmentTestsBase {
        protected TestEnvironmentConfig TestEnvironmentConfig { get; set; }

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
    }
}