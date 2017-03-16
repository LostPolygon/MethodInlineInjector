using System;
using System.Collections.Generic;
using System.Linq;
using devtm.Cecil.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LostPolygon.MethodInlineInjector {
    public class MethodDefinitionCloner {
        private readonly Dictionary<VariableDefinition, VariableDefinition> _variableMap = new Dictionary<VariableDefinition, VariableDefinition>();
        private readonly int[] _instructionOperandMap;
        private readonly int[][] _instructionArrayOperandMap;

        public MethodDefinition SourceMethod { get; }
        public MethodDefinition TargetMethod { get; }
        public ModuleDefinition TargetModule { get; }

        public MethodDefinitionCloner(MethodDefinition sourceMethod, MethodDefinition targetMethod, ModuleDefinition targetModule) {
            SourceMethod = sourceMethod;
            TargetMethod = targetMethod;
            TargetModule = targetModule;

            _instructionOperandMap = new int[SourceMethod.Body.Instructions.Count];
            for (int i = 0; i < _instructionOperandMap.Length; i++) {
                _instructionOperandMap[i] = -2;
            }

            _instructionArrayOperandMap = new int[SourceMethod.Body.Instructions.Count][];
        }

        public void Clone() {
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

            for (int i = 0; i < _instructionArrayOperandMap.Length; i++) {
                if (_instructionArrayOperandMap[i] == null)
                    continue;

                TargetMethod.Body.Instructions[i].Operand =
                    _instructionArrayOperandMap[i]
                    .Select(instructionIndex => TargetMethod.Body.Instructions[instructionIndex])
                    .ToArray();
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

        protected virtual FieldReference ImportFieldReference(FieldReference operand) {
            return TargetModule.Import(operand);
        }

        protected virtual Instruction CloneInstruction(Instruction instruction, int instructionIndex) {
            Instruction cloneInstruction;
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
            } else if (operand is Instruction[] instructionArrayOperand) {
                cloneInstruction = CloneInstructionOperandInstructionArray(instruction, instructionArrayOperand, instructionIndex);
            } else {
                throw new MethodInlineInjectorException($"Unknown operand type {operand.GetType()}");
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
            FieldDefinition fieldDefinition = ImportFieldReference(operand).Resolve();
            if (fieldDefinition == null)
                throw new MethodInlineInjectorException($"Field '{operand}' not found in the source method module");

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

        private Instruction CloneInstructionOperandInstructionArray(Instruction sourceInstruction, Instruction[] operand, int instructionIndex) {
            _instructionArrayOperandMap[instructionIndex] =
                operand
                .Select(operandInstruction => SourceMethod.Body.Instructions.IndexOf(operandInstruction))
                .ToArray();

            return Instruction.Create(sourceInstruction.OpCode, Array.Empty<Instruction>());
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