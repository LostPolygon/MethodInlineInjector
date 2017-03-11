using System.IO;
using NUnit.Framework;
using TestInjectedLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class SmokeTests : IntegrationTestMainBase {
        [Test]
        [SaveModifiedAssemblies]
        public void ComplexToMonoCecil() {
            const string sourceAssemblyName = "Mono.Cecil.dll";
            const string targetAssemblyName = "Mono.Cecil_Injectee.dll";

            File.Copy(sourceAssemblyName, targetAssemblyName, true);

            InjectionConfiguration configuration = new InjectionConfiguration();
            configuration.InjecteeAssemblies.Add(new InjectionConfiguration.InjecteeAssembly(targetAssemblyName));
            configuration.InjectedMethods.Add(
                    new InjectionConfiguration.InjectedMethod(
                        InjectedLibraryPath,
                        $"{InjectedClassName}.{nameof(TestInjectedMethods.ComplexMethod)}"
                    )
            );

            ExecuteSimpleInjection(configuration);
        }

        [Test]
        [SaveModifiedAssemblies]
        public void ComplexToMonoCecilAtReturn() {
            const string sourceAssemblyName = "Mono.Cecil.dll";
            const string targetAssemblyName = "Mono.Cecil_Injectee_AtReturn.dll";

            File.Copy(sourceAssemblyName, targetAssemblyName, true);

            InjectionConfiguration configuration = new InjectionConfiguration();
            configuration.InjecteeAssemblies.Add(new InjectionConfiguration.InjecteeAssembly(targetAssemblyName));
            configuration.InjectedMethods.Add(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(TestInjectedMethods.ComplexMethod)}",
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn
                )
            );

            ExecuteSimpleInjection(configuration);
        }
    }
}