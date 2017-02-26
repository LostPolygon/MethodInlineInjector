using System;
using System.Collections.Generic;
using System.Linq;
using devtm.Cecil;
using devtm.Cecil.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace LostPolygon.MethodInlineInjector {
    internal class MethodInlineInjector {
        private readonly CompiledInjectionConfiguration _compiledInjectionConfiguration;

        public MethodInlineInjector(CompiledInjectionConfiguration compiledInjectionConfiguration) {
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
            // Unroll short form instructions so they can be auto-fixed by Cecil
            // automatically when new instructions are inserted
            injectedMethod.Body.SimplifyMacros();
            injecteeMethod.Body.SimplifyMacros();

            injecteeMethod.Body.InitLocals |= injectedMethod.Body.InitLocals;

            ILProcessor injecteeIlProcessor = injecteeMethod.Body.GetILProcessor();
            if (injectionPosition == InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart) {
                // Inject variables to the beginning of the variable list
                injecteeMethod.Body.Variables.InsertRangeToStart(injectedMethod.Body.Variables);

                // First instruction of the injectee method. Instruction of the injected methods are inserted before it
                Instruction injecteeFirstInstruction = injecteeMethod.Body.Instructions[0];
                Instruction injectedLastInstruction = injectedMethod.Body.Instructions.Last();

                // Insert injected method to the beginning
                for (int i = 0; i < injectedMethod.Body.Instructions.Count; i++) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertBefore(injecteeFirstInstruction, injectedInstruction);
                }

                // Clone exception handlers
                injecteeMethod.Body.ExceptionHandlers.AddRange(injectedMethod.Body.ExceptionHandlers);

                // Replace Ret from the end of the injected method with Nop, 
                // so the execution could go to injectee code after the injected method end
                injectedLastInstruction = injecteeIlProcessor.ReplaceAndFixReferences(injectedLastInstruction, Instruction.Create(OpCodes.Nop), injecteeMethod);  
            } else if (injectionPosition == InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodReturn) {
                // Inject variables to the end of the variable list
                injecteeMethod.Body.Variables.AddRange(injectedMethod.Body.Variables);
                
                // Ret instruction at the end of the injectee method must be replace with Nop, 
                // so the execution could go to the injected code after the injecteed method end
                Instruction injecteeLastInstruction = injecteeMethod.Body.Instructions.Last();

                // Append injected method to the end
                for (int i = injectedMethod.Body.Instructions.Count - 1; i >= 0 ; i--) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertAfter(injecteeLastInstruction, injectedInstruction);
                }

                // Clone exception handlers
                injecteeMethod.Body.ExceptionHandlers.AddRange(injectedMethod.Body.ExceptionHandlers);

                // Replace Ret from the end of the injectee method with Nop, 
                // so the execution could go to injected code after the injectee method end
                injecteeLastInstruction = injecteeIlProcessor.ReplaceAndFixReferences(injecteeLastInstruction, Instruction.Create(OpCodes.Nop), injecteeMethod);
            }

            injecteeMethod.Body.OptimizeMacros();
        }

        private static MethodDefinition CloneAndImportMethod(MethodDefinition sourceMethod, AssemblyDefinition targetAssembly) {
            TypeReference importedReturnTypeReference = targetAssembly.MainModule.Import(sourceMethod.ReturnType);
            MethodDefinition clonedMethod = new MethodDefinition(sourceMethod.Name, sourceMethod.Attributes, importedReturnTypeReference);
            clonedMethod.DeclaringType = targetAssembly.MainModule.Types[0];
            MethodCloner methodCloner = new MethodClonerValidated(sourceMethod, clonedMethod, targetAssembly.MainModule);
            methodCloner.Clone();

            return clonedMethod;
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

            private void ValidateTypeReference(TypeReference type) {
                if (type.Scope == SourceMethod.Module)
                    throw new MethodInlineInjectorException(
                        $"Type '{type.FullName}' was attempted to be imported from injected assembly " +
                        $"'{SourceMethod.Module.Assembly}' to the injectee assembly '{TargetMethod.Module.Assembly}'");
            }
        }

        private class MethodCloner {
            private readonly Dictionary<VariableDefinition, VariableDefinition> _variableMap = new Dictionary<VariableDefinition, VariableDefinition>();
            private readonly int[] _instructionOperandMap;
            private bool _executed;

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
                if (_executed)
                    throw new InvalidOperationException();

                _executed = true;

                // Clone local variables
                foreach (VariableDefinition variable in SourceMethod.Body.Variables) {
                    VariableDefinition variableDefinition = new VariableDefinition(variable.Name, ImportTypeReference(variable.VariableType));
                    TargetMethod.Body.Variables.Add(variableDefinition);
                    _variableMap.Add(variable, variableDefinition);
                }

                // Clone body instructions
                for (int i = 0; i < SourceMethod.Body.Instructions.Count; i++) {
                    Instruction instruction = SourceMethod.Body.Instructions[i];
                    Instruction clonedInstruction = CloneInstruction(instruction, i);

                    instruction.SequencePoint?.CopyTo(clonedInstruction);
                    TargetMethod.Body.Instructions.Add(clonedInstruction);
                }

                // Map instructions that are operands of other instructions to newly created clones
                for (int i = 0; i < _instructionOperandMap.Length; i++) {
                    if (_instructionOperandMap[i] < 0)
                        continue;

                    TargetMethod.Body.Instructions[i].Operand = TargetMethod.Body.Instructions[_instructionOperandMap[i]];
                }

                // Clone exception handlers
                foreach (ExceptionHandler sourceExceptionHandler in SourceMethod.Body.ExceptionHandlers) {
                    ExceptionHandler cloneExceptionHandler = new ExceptionHandler(sourceExceptionHandler.HandlerType) {
                        CatchType = sourceExceptionHandler.CatchType != null ? ImportTypeReference(sourceExceptionHandler.CatchType) : null,
                        FilterStart = GetMatchingInstructionByIndex(sourceExceptionHandler.FilterStart),
                        HandlerStart = GetMatchingInstructionByIndex(sourceExceptionHandler.HandlerStart),
                        HandlerEnd = GetMatchingInstructionByIndex(sourceExceptionHandler.HandlerEnd),
                        TryStart = GetMatchingInstructionByIndex(sourceExceptionHandler.TryStart),
                        TryEnd = GetMatchingInstructionByIndex(sourceExceptionHandler.TryEnd)
                    };

                    TargetMethod.Body.ExceptionHandlers.Add(cloneExceptionHandler);
                }
            }

            protected virtual TypeReference ImportTypeReference(TypeReference operand) {
                return TargetModule.Import(operand);
            }

            protected virtual MethodReference ImportMethodReference(MethodReference operand) {
                return TargetModule.Import(operand);
            }

            protected virtual Instruction CloneInstruction(Instruction instruction, int instructionIndex) {
                Instruction cloneInstruction = null;
                object operand = instruction.Operand;
                if (operand == null) {
                    cloneInstruction = CloneNoOperandInstruction(instruction);
                } else if (operand is byte byteOperand) {
                    cloneInstruction = CloneByteOperandInstruction(instruction, byteOperand);
                } else if (operand is sbyte sbyteOperand) {
                    cloneInstruction = CloneSByteOperandInstruction(instruction, sbyteOperand);
                } else if (operand is int intOperand) {
                    cloneInstruction = CloneIntOperandInstruction(instruction, intOperand);
                } else if (operand is long longOperand) {
                    cloneInstruction = CloneLongOperandInstruction(instruction, longOperand);
                } else if (operand is float floatOperand) {
                    cloneInstruction = CloneFloatOperandInstruction(instruction, floatOperand);
                } else if (operand is double doubleOperand) {
                    cloneInstruction = CloneDoubleOperandInstruction(instruction, doubleOperand);
                } else if (operand is string stringOperand) {
                    cloneInstruction = CloneStringOperandInstruction(instruction, stringOperand);
                } else if (operand is TypeReference typeReferenceOperand) {
                    cloneInstruction = CloneTypeReferenceOperandInstruction(instruction, typeReferenceOperand);
                } else if (operand is FieldReference fieldReferenceOperand) {
                    cloneInstruction = CloneFieldReferenceOperandInstruction(instruction, fieldReferenceOperand);
                } else if (operand is MethodReference methodReferenceOperand) {
                    cloneInstruction = CloneMethodReferenceOperandInstruction(instruction, methodReferenceOperand);
                } else if (operand is ParameterDefinition parameterDefinitionOperand) {
                    cloneInstruction = CloneParameterDefinitionOperandInstruction(instruction, parameterDefinitionOperand);
                } else if (operand is VariableDefinition variableDefinitionOperand) {
                    cloneInstruction = CloneVariableDefinitionOperandInstruction(instruction, variableDefinitionOperand);
                } else if (operand is Instruction instructionOperand) {
                    cloneInstruction = CloneInstructionOperandInstruction(instruction, instructionOperand, instructionIndex);
                } else {
                    throw new MethodInlineInjectorException("Unknown operand type");
                }

                cloneInstruction.Offset = instruction.Offset;
                return cloneInstruction;
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
                    throw new MethodInlineInjectorException("The block can't be copied because some fields do not exist in the target method declaring type");

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

            protected virtual Instruction CloneInstructionOperandInstruction(Instruction sourceInstruction, Instruction operand, int instructionIndex) {
                _instructionOperandMap[instructionIndex] = SourceMethod.Body.Instructions.IndexOf(operand);
                return Instruction.Create(sourceInstruction.OpCode, Instruction.Create(OpCodes.Nop));
            }

            private Instruction GetMatchingInstructionByIndex(Instruction sourceInstruction) {
                if (sourceInstruction == null)
                    return null;

                int sourceIndex = SourceMethod.Body.Instructions.IndexOf(sourceInstruction);
                if (sourceIndex == -1)
                    throw new MethodInlineInjectorException("Matching instruction not found in source method");

                return TargetMethod.Body.Instructions[sourceIndex];
            }
        }
    }
}