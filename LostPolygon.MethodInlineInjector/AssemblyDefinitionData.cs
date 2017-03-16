using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace LostPolygon.MethodInlineInjector {
    public class AssemblyDefinitionData {
        public AssemblyDefinition AssemblyDefinition { get; }
        public IReadOnlyList<TypeDefinition> AllTypes { get; }
        public IReadOnlyList<MethodDefinition> AllMethods { get; }

        public AssemblyDefinitionData(AssemblyDefinition assemblyDefinition) {
            AssemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));

            AllTypes = assemblyDefinition.MainModule.GetAllTypes().ToList();
            AllMethods = AllTypes.SelectMany(type => type.Methods).ToList();
        }
    }
}