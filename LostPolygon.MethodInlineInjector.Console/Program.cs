using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using LostPolygon.MethodInlineInjector.Configuration;

namespace LostPolygon.MethodInlineInjector {
    internal class Program {
        public static void Main(string[] args) {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // TODO: args validation, stdin config input
            string serializedInjectorConfiguration = File.ReadAllText(args[0]);
            InjectionConfiguration injectionConfiguration = XmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(serializedInjectorConfiguration);
            
            CompiledInjectionConfigurationFactory compiledInjectionConfigurationFactory = new CompiledInjectionConfigurationFactory(injectionConfiguration);
            CompiledInjectionConfiguration compiledInjectionConfiguration = compiledInjectionConfigurationFactory.Build();

            MethodInlineInjector assemblyMethodInjector = new MethodInlineInjector(compiledInjectionConfiguration);

            Stopwatch sw = Stopwatch.StartNew();
            assemblyMethodInjector.Inject();
            sw.Stop();
            Console.WriteLine("Injected in {0} ms", sw.ElapsedMilliseconds);

            foreach (CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly in compiledInjectionConfiguration.InjecteeAssemblies) {
                injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.Write(injecteeAssembly.SourceInjecteeAssembly.AssemblyPath);
            }
        }
    }
}