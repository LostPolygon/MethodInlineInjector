using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;

namespace LostPolygon.AssemblyMethodInlineInjector {
    internal class CompiledInjectionConfiguration {
        public ReadOnlyCollection<InjectedAssemblyMethods> InjectedMethods { get; }
        public ReadOnlyCollection<InjecteeAssembly>  InjecteeAssemblies { get; }

        public CompiledInjectionConfiguration(ReadOnlyCollection<InjectedAssemblyMethods> injectedMethods, ReadOnlyCollection<InjecteeAssembly> injecteeAssemblies) {
            InjectedMethods = injectedMethods;
            InjecteeAssemblies = injecteeAssemblies;
        }

        public class InjectedAssemblyMethods {
            public AssemblyDefinition AssemblyDefinition { get; }
            public ReadOnlyCollection<InjectedMethod> Methods { get; }

            public InjectedAssemblyMethods(AssemblyDefinition assemblyDefinition, ReadOnlyCollection<InjectedMethod> methods) {
                AssemblyDefinition = assemblyDefinition;
                Methods = methods;
            }
        }

        public class InjectedMethod {
            public InjectionConfiguration.InjectedMethod SourceInjectedMethod { get; }
            public MethodDefinition MethodDefinition { get; }

            public InjectedMethod(InjectionConfiguration.InjectedMethod sourceInjectedMethod, MethodDefinition methodDefinition) {
                SourceInjectedMethod = sourceInjectedMethod;
                MethodDefinition = methodDefinition;
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