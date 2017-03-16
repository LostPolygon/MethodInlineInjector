using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class SmokeTests : IntegrationTestMainBase {
        [Test]
        [SaveModifiedAssemblies]
        public void InjectComplexToMonoCecil() {
            const string sourceAssemblyName = "Mono.Cecil.dll";
            const string targetAssemblyName = "Mono.Cecil_Injectee.dll";

            File.Copy(sourceAssemblyName, targetAssemblyName, true);

            InjectionConfiguration configuration = new InjectionConfiguration(
                new List<InjecteeAssembly> {
                    new InjecteeAssembly(
                        targetAssemblyName,
                        null,
                        IntegrationTestsHelper.GetStandardAssemblyReferenceWhitelist().AsReadOnly()
                    )
                }.AsReadOnly(),
                new List<InjectedMethod> {
                    new InjectedMethod(
                        InjectedLibraryPath,
                        $"{InjectedClassName}.{nameof(TestInjectedMethods.Complex)}"
                    )
                }.AsReadOnly()
            );

            ExecuteSimpleInjection(configuration);
        }

        [Test]
        [SaveModifiedAssemblies]
        public void InjectComplexToMonoCecilAtReturn() {
            const string sourceAssemblyName = "Mono.Cecil.dll";
            const string targetAssemblyName = "Mono.Cecil_Injectee_AtReturn.dll";

            File.Copy(sourceAssemblyName, targetAssemblyName, true);

            InjectionConfiguration configuration = new InjectionConfiguration(
                new List<InjecteeAssembly> {
                    new InjecteeAssembly(
                        targetAssemblyName,
                        null,
                        IntegrationTestsHelper.GetStandardAssemblyReferenceWhitelist().AsReadOnly()
                    )
                }.AsReadOnly(),
                new List<InjectedMethod> {
                    new InjectedMethod(
                        InjectedLibraryPath,
                        $"{InjectedClassName}.{nameof(TestInjectedMethods.Complex)}",
                        MethodInjectionPosition.InjecteeMethodReturn
                    )
                }.AsReadOnly()
            );

            ExecuteSimpleInjection(configuration);
        }
    }
}