using System;
using System.Globalization;
using System.IO;
using System.Threading;

using LostPolygon.AssemblyMethodInjector.Configuration;

namespace LostPolygon.AssemblyMethodInjector {
    internal class Program {
        private static void Main(string[] args) {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            string serializedInjectorConfiguration = File.ReadAllText(args[0]);
            InjectionConfiguration injectionConfiguration = XmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(serializedInjectorConfiguration);
            
            CompiledInjectionConfigurationBuilder compiledInjectionConfigurationBuilder = new CompiledInjectionConfigurationBuilder(injectionConfiguration);
            CompiledInjectionConfiguration compiledInjectionConfiguration = compiledInjectionConfigurationBuilder.Build();

            AssemblyMethodInjector assemblyMethodInjector = new AssemblyMethodInjector(compiledInjectionConfiguration);
            assemblyMethodInjector.Inject();

            foreach (CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly in compiledInjectionConfiguration.InjecteeAssemblies) {
                injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.Write(injecteeAssembly.SourceInjecteeAssembly.AssemblyPath);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}