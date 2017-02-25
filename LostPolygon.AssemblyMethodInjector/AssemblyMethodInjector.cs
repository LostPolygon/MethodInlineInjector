using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using devtm.Cecil;
using devtm.Cecil.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace LostPolygon.AssemblyMethodInjector {
    internal class AssemblyMethodInjector {
        private readonly CompiledInjectionConfiguration _compiledInjectionConfiguration;

        public AssemblyMethodInjector(CompiledInjectionConfiguration compiledInjectionConfiguration) {
            _compiledInjectionConfiguration = compiledInjectionConfiguration;
        }

        public void Inject() {
            foreach (CompiledInjectionConfiguration.InjecteeAssembly injecteeAssembly in _compiledInjectionConfiguration.InjecteeAssemblies) {
                foreach (CompiledInjectionConfiguration.InjectedAssemblyMethods injectedAssemblyMethodsTuple in _compiledInjectionConfiguration.InjectedMethods) {
                    List<AssemblyNameReference> injecteeAssemblyNameReferences =
                        injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition.MainModule.AssemblyReferences
                            .ToList();

                    List<TypeReference> injectedTypeReferences =
                        injectedAssemblyMethodsTuple.AssemblyDefinition.MainModule.GetTypeReferences()
                            .ToList();

                    foreach (TypeReference injectedTypeReference in injectedTypeReferences) {
                        // TODO: add strict name check mode
                        AssemblyNameReference matchingAssemblyNameReference =
                            GetMatchingAssemblyNameReference(injectedTypeReference.Scope, injecteeAssemblyNameReferences);

                        // TODO: implement whitelist of assemblies that can be added
                        if (matchingAssemblyNameReference == null) {
                            Console.WriteLine(
                                $"Injectee assembly '{injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition} ' " +
                                $"has no match for assembly reference '{injectedTypeReference.Scope}', " +
                                $"the reference will be added"
                            );
                        } else {
                            injectedTypeReference.Scope = matchingAssemblyNameReference;
                        }
                    }

                    foreach (MethodDefinition injecteeMethod in injecteeAssembly.InjecteeMethodsDefinitions) {
                        foreach (CompiledInjectionConfiguration.InjectedMethod injectedMethod in injectedAssemblyMethodsTuple.Methods) {
                            MethodDefinition importedInjectedMethod = CloneAndImportMethod(injectedMethod.MethodDefinition, injecteeAssembly.AssemblyDefinitionData.AssemblyDefinition);
                            InjectMethod(importedInjectedMethod, injecteeMethod, injectedMethod.SourceInjectedMethod.InjectionPosition);
                        }
                    }
                }
            }
        }

        private static AssemblyNameReference GetMatchingAssemblyNameReference(IMetadataScope injectedAssemblyNameReference, List<AssemblyNameReference> injecteeAssemblyNameReferences) {
            foreach (AssemblyNameReference injecteeAssemblyNameReference in injecteeAssemblyNameReferences) {
                if (injectedAssemblyNameReference.Name == injecteeAssemblyNameReference.Name) {
                    return injecteeAssemblyNameReference;
                }
            }

            return null;
        }

        private static void InjectMethod(MethodDefinition injectedMethod, MethodDefinition injecteeMethod, InjectionConfiguration.InjectedMethod.MethodInjectionPosition injectionPosition) {
            injectedMethod.Body.SimplifyMacros();
            injecteeMethod.Body.SimplifyMacros();

            if (injectedMethod.Body.Variables.Count > 0) {
                injecteeMethod.Body.InitLocals = true;
            }

            ILProcessor injecteeIlProcessor = injecteeMethod.Body.GetILProcessor();
            if (injectionPosition == InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart) {
                // Inject variables to the beginning of the variable list
                injecteeMethod.Body.Variables.InsertRangeToStart(injectedMethod.Body.Variables);
                Instruction injecteeFirstInstruction = injecteeMethod.Body.Instructions[0];
                Instruction injectedLastRetInstruction = injectedMethod.Body.Instructions.Last(instruction => instruction.OpCode == OpCodes.Ret);

                for (int i = 0; i < injectedMethod.Body.Instructions.Count; i++) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertBefore(injecteeFirstInstruction, injectedInstruction);
                }

                injecteeFirstInstruction = Instruction.Create(OpCodes.Nop);
                injecteeIlProcessor.Replace(injectedLastRetInstruction, injecteeFirstInstruction);
                foreach (Instruction bodyInstruction in injecteeMethod.Body.Instructions) {
                    if (bodyInstruction.Operand == injectedLastRetInstruction) {
                        bodyInstruction.Operand = injecteeFirstInstruction;
                    }
                }
            } else if (injectionPosition == InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn) {
                injecteeMethod.Body.Variables.AddRange(injectedMethod.Body.Variables);

                // Remove Ret from the end of the injectee method, so the execution could go to injected code after the injecteed method end
                Instruction injecteeLastRetInstruction = injecteeMethod.Body.Instructions.Last(instruction => instruction.OpCode == OpCodes.Ret);
                Instruction injecteeNewLastInstruction = Instruction.Create(OpCodes.Nop);
                injecteeIlProcessor.Replace(injecteeLastRetInstruction, injecteeNewLastInstruction);
                foreach (Instruction bodyInstruction in injecteeMethod.Body.Instructions) {
                    if (bodyInstruction.Operand == injecteeLastRetInstruction) {
                        bodyInstruction.Operand = injecteeNewLastInstruction;
                    }
                }

                Instruction injecteeLastInstruction = injecteeMethod.Body.Instructions.Last();

                // Append injected method to the end
                for (int i = injectedMethod.Body.Instructions.Count - 1; i >= 0 ; i--) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertAfter(injecteeLastInstruction, injectedInstruction);
                }

                injecteeMethod.Body.OptimizeMacros();
            }

            injecteeMethod.Body.OptimizeMacros();
        }

        public static MethodDefinition CloneAndImportMethod(MethodDefinition sourceMethod, AssemblyDefinition declaredAssembly) {
            TypeReference importedReturnTypeReference = declaredAssembly.MainModule.Import(sourceMethod.ReturnType);
            MethodDefinition importedInjectedMethod = new MethodDefinition(sourceMethod.Name, sourceMethod.Attributes, importedReturnTypeReference);
            importedInjectedMethod.DeclaringType = declaredAssembly.MainModule.Types[0];
            MethodCloner methodCloner = new MethodClonerValidated(sourceMethod, importedInjectedMethod, declaredAssembly.MainModule);
            methodCloner.Clone();

            return importedInjectedMethod;
        }

        private class MethodClonerValidated : MethodCloner {
            public MethodClonerValidated(MethodDefinition sourceMethod, MethodDefinition targetMethod, ModuleDefinition targetModule) 
                : base(sourceMethod, targetMethod, targetModule) {
            }

            protected override TypeReference ImportTypeReference(TypeReference operand) {
                ValidateTypeReference(operand);
                return base.ImportTypeReference(operand);
            }

            protected override MethodReference ImportMethodReference(MethodReference operand) {
                ValidateTypeReference(operand.DeclaringType);
                ValidateTypeReference(operand.ReturnType);
                foreach (ParameterDefinition parameterDefinition in operand.Parameters) {
                    ValidateTypeReference(parameterDefinition.ParameterType);
                }

                foreach (GenericParameter genericParameter in operand.GenericParameters) {
                    ValidateTypeReference(genericParameter.DeclaringType);
                }

                return base.ImportMethodReference(operand);
            }

            protected override TypeReference ImportVariableDefinition(VariableDefinition operand) {
                ValidateTypeReference(operand.VariableType);
                return base.ImportVariableDefinition(operand);
            }

            private void ValidateTypeReference(TypeReference type) {
                if (type.Scope == SourceMethod.Module)
                    throw new AssemblyMethodInjectorException(
                        $"Type '{type.FullName}' was attempted to be imported from injected assembly " +
                        $"'{SourceMethod.Module.Assembly}' to the injectee assembly '{TargetMethod.Module.Assembly}'");
            }
        }

        private class MethodCloner {
            private readonly Dictionary<VariableDefinition, VariableDefinition> _variableMap = new Dictionary<VariableDefinition, VariableDefinition>();
            private readonly int[] _instructionOperandMap;

            public MethodDefinition SourceMethod { get; }
            public MethodDefinition TargetMethod { get; }
            public ModuleDefinition TargetModule { get; }

            public MethodCloner(MethodDefinition sourceMethod, MethodDefinition targetMethod, ModuleDefinition targetModule) {
                SourceMethod = sourceMethod;
                TargetMethod = targetMethod;
                TargetModule = targetModule;

                _instructionOperandMap = new int[SourceMethod.Body.Instructions.Count];
                for (int i = 0; i < _instructionOperandMap.Length; i++) {
                    _instructionOperandMap[i] = -2;
                }
            }

            public void Clone() {
                foreach (VariableDefinition variable in SourceMethod.Body.Variables) {
                    VariableDefinition variableDefinition = new VariableDefinition(variable.Name, ImportVariableDefinition(variable));
                    TargetMethod.Body.Variables.Add(variableDefinition);
                    _variableMap.Add(variable, variableDefinition);
                }

                foreach (Instruction instruction in SourceMethod.Body.Instructions) {
                    Instruction clonedInstruction = CloneInstruction(instruction);

                    instruction.SequencePoint?.CopyTo(clonedInstruction);
                    TargetMethod.Body.Instructions.Add(clonedInstruction);
                }

                for (int i = 0; i < _instructionOperandMap.Length; i++) {
                    if (_instructionOperandMap[i] < 0)
                        continue;

                    TargetMethod.Body.Instructions[i].Operand = TargetMethod.Body.Instructions[_instructionOperandMap[i]];
                }
            }

            protected virtual TypeReference ImportTypeReference(TypeReference operand) {
                return TargetModule.Import(operand);
            }

            protected virtual MethodReference ImportMethodReference(MethodReference operand) {
                return TargetModule.Import(operand);
            }

            protected virtual TypeReference ImportVariableDefinition(VariableDefinition operand) {
                return TargetModule.Import(operand.VariableType);
            }

            protected virtual Instruction CloneInstruction(Instruction sourceInstruction) {
                Instruction clonedInstruction = null;
                object operand = sourceInstruction.Operand;
                if (operand == null) {
                    clonedInstruction = CloneNoOperandInstruction(sourceInstruction);
                } else if (operand is byte) {
                    clonedInstruction = CloneByteOperandInstruction(sourceInstruction, (byte) operand);
                } else if (operand is sbyte) {
                    clonedInstruction = CloneSByteOperandInstruction(sourceInstruction, (sbyte) operand);
                } else if (operand is int) {
                    clonedInstruction = CloneIntOperandInstruction(sourceInstruction, (int) operand);
                } else if (operand is long) {
                    clonedInstruction = CloneLongOperandInstruction(sourceInstruction, (long) operand);
                } else if (operand is float) {
                    clonedInstruction = CloneFloatOperandInstruction(sourceInstruction, (float) operand);
                } else if (operand is double) {
                    clonedInstruction = CloneDoubleOperandInstruction(sourceInstruction, (double) operand);
                } else if (operand is string) {
                    clonedInstruction = CloneStringOperandInstruction(sourceInstruction, (string) operand);
                }

                TypeReference typeReferenceOperand = operand as TypeReference;
                if (typeReferenceOperand != null) {
                    clonedInstruction = CloneTypeReferenceOperandInstruction(sourceInstruction, typeReferenceOperand);
                }

                FieldReference fieldReferenceOperand = operand as FieldReference;
                if (fieldReferenceOperand != null) {
                    clonedInstruction = CloneFieldReferenceOperandInstruction(sourceInstruction, fieldReferenceOperand);
                }

                MethodReference methodReferenceOperand = operand as MethodReference;
                if (methodReferenceOperand != null) {
                    clonedInstruction = CloneMethodReferenceOperandInstruction(sourceInstruction, methodReferenceOperand);
                }

                ParameterDefinition parameterDefinitionOperand = operand as ParameterDefinition;
                if (parameterDefinitionOperand != null) {
                    clonedInstruction = CloneParameterDefinitionOperandInstruction(sourceInstruction, parameterDefinitionOperand);
                }

                VariableDefinition variableDefinitionOperand = operand as VariableDefinition;
                if (variableDefinitionOperand != null) {
                    clonedInstruction = CloneVariableDefinitionOperandInstruction(sourceInstruction, variableDefinitionOperand);
                }

                Instruction instructionOperand = operand as Instruction;
                if (instructionOperand != null) {
                    clonedInstruction = CloneInstructionOperandInstruction(sourceInstruction, instructionOperand);
                }

                if (clonedInstruction == null)
                    throw new AssemblyMethodInjectorException("Unknown operand type");

                clonedInstruction.Offset = sourceInstruction.Offset;
                return clonedInstruction;
            }

            protected virtual Instruction CloneNoOperandInstruction(Instruction sourceInstruction) {
                return Instruction.Create(sourceInstruction.OpCode);
            }

            protected virtual Instruction CloneByteOperandInstruction(Instruction sourceInstruction, byte operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneSByteOperandInstruction(Instruction sourceInstruction, sbyte operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneIntOperandInstruction(Instruction sourceInstruction, int operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneLongOperandInstruction(Instruction sourceInstruction, long operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneFloatOperandInstruction(Instruction sourceInstruction, float operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneDoubleOperandInstruction(Instruction sourceInstruction, double operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneStringOperandInstruction(Instruction sourceInstruction, string operand) {
                return Instruction.Create(sourceInstruction.OpCode, operand);
            }

            protected virtual Instruction CloneTypeReferenceOperandInstruction(Instruction sourceInstruction, TypeReference operand) {
                return Instruction.Create(sourceInstruction.OpCode, ImportTypeReference(operand));
            }

            protected virtual Instruction CloneFieldReferenceOperandInstruction(Instruction sourceInstruction, FieldReference operand) {
                FieldDefinition fieldDefinition = null;
                foreach (FieldDefinition field in TargetMethod.DeclaringType.Fields) {
                    if (field.Name != operand.Name) 
                        continue;

                    fieldDefinition = field;
                    break;
                }

                if (fieldDefinition == null)
                    throw new AssemblyMethodInjectorException("The block can't be copied because some fields do not exist in the target method declaring type");

                return Instruction.Create(sourceInstruction.OpCode, fieldDefinition);
            }

            protected virtual Instruction CloneMethodReferenceOperandInstruction(Instruction sourceInstruction, MethodReference operand) {
                return Instruction.Create(sourceInstruction.OpCode, ImportMethodReference(operand));
            }

            protected virtual Instruction CloneParameterDefinitionOperandInstruction(Instruction sourceInstruction, ParameterDefinition operand) {
                return Instruction.Create(sourceInstruction.OpCode, TargetMethod.Parameters[operand.Index]);
            }

            protected virtual Instruction CloneVariableDefinitionOperandInstruction(Instruction sourceInstruction, VariableDefinition operand) {
                return Instruction.Create(sourceInstruction.OpCode, _variableMap[operand]);
            }

            protected virtual Instruction CloneInstructionOperandInstruction(Instruction sourceInstruction, Instruction operand) {
                _instructionOperandMap[SourceMethod.Body.Instructions.IndexOf(sourceInstruction)] = SourceMethod.Body.Instructions.IndexOf(operand);
                return Instruction.Create(sourceInstruction.OpCode, CloneInstruction(operand));
            }
        }
    }
}