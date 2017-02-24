using System.Collections.ObjectModel;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace LostPolygon.AssemblyMethodInjector {
    internal class AssemblyDefinitionData {
        public AssemblyDefinition AssemblyDefinition { get; }
        public ReadOnlyCollection<TypeDefinition> AllTypes { get; }
        public ReadOnlyCollection<MethodDefinition> AllMethods { get; }

        public AssemblyDefinitionData(AssemblyDefinition assemblyDefinition) {
            AssemblyDefinition = assemblyDefinition;

            AllTypes = assemblyDefinition.MainModule.GetAllTypes().ToList().AsReadOnly();
            AllMethods = AllTypes.SelectMany(typeDefinition => typeDefinition.Methods).ToList().AsReadOnly();
        }
    }
}