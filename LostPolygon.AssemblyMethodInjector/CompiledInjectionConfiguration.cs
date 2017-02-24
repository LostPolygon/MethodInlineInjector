using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;

namespace LostPolygon.AssemblyMethodInjector {
    internal class CompiledInjectionConfiguration {
        public ReadOnlyCollection<InjectedAssemblyMethods> InjectedMethods { get; }
        public ReadOnlyCollection<InjecteeAssembly>  InjecteeAssemblies { get; }

        public CompiledInjectionConfiguration(ReadOnlyCollection<InjectedAssemblyMethods> injectedMethods, ReadOnlyCollection<InjecteeAssembly> injecteeAssemblies) {
            InjectedMethods = injectedMethods;
            InjecteeAssemblies = injecteeAssemblies;
        }

        public class InjectedAssemblyMethods {
            public AssemblyDefinition AssemblyDefinition { get; }
            public ReadOnlyCollection<MethodDefinition> MethodDefinitions { get; }

            public InjectedAssemblyMethods(AssemblyDefinition assemblyDefinition, ReadOnlyCollection<MethodDefinition> methodDefinitions) {
                AssemblyDefinition = assemblyDefinition;
                MethodDefinitions = methodDefinitions;
            }
        }

        public class InjecteeAssembly {
            public InjectionConfiguration.InjecteeAssembly SourceInjecteeAssembly { get; }

            public AssemblyDefinitionData AssemblyDefinitionData { get; }

            public List<MethodDefinition> InjecteeMethodsDefinitions { get; }

            public InjecteeAssembly(InjectionConfiguration.InjecteeAssembly sourceInjecteeAssembly, AssemblyDefinitionData assemblyDefinitionData, List<MethodDefinition> injecteeMethodsDefinitions) {
                SourceInjecteeAssembly = sourceInjecteeAssembly;
                AssemblyDefinitionData = assemblyDefinitionData;
                InjecteeMethodsDefinitions = injecteeMethodsDefinitions;
            }
        }
    }
}