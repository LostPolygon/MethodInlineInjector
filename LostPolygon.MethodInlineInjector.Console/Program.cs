using System.Globalization;
using System.IO;
using System.Threading;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    internal class Program {
        public static void Main(string[] args) {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // TODO: args validation, stdin config input
            string serializedInjectorConfiguration = File.ReadAllText(args[0]);
            InjectionConfiguration injectionConfiguration =
                SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(serializedInjectorConfiguration);

            ResolvedInjectionConfiguration resolvedInjectionConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(injectionConfiguration);

            MethodInlineInjector assemblyMethodInjector = new MethodInlineInjector(resolvedInjectionConfiguration);
            assemblyMethodInjector.Inject();

            foreach (ResolvedInjecteeAssembly injecteeAssembly in resolvedInjectionConfiguration.InjecteeAssemblies) {
                injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.Write(injecteeAssembly.SourceInjecteeAssembly.AssemblyPath);
            }
        }
    }
}