using System.IO;

namespace LostPolygon.MethodInlineInjector.Tests {
    public class IntegrationTestMainBase : IntegrationTestsBase {
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

        public override string InjectedClassName => typeof(TestInjectedLibrary.TestInjectedMethods).FullName;
        public override string InjecteeClassName => typeof(TestInjecteeLibrary.TestInjectee).FullName;

        protected override string[] CommonFiles => new[] {
            "TestData/Common/RuntimeAssembliesWhitelist.xml",
            "TestData/Common/UnityPerfomanceWhitelist.xml"
        };
    }
}