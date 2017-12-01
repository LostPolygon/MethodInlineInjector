using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LostPolygon.MethodInlineInjector {
    public class MethodDefinitionCloner {
        private readonly Instruction[] _instructions;

        public MethodDefinition SourceMethod { get; }
        public MethodDefinition TargetMethod { get; }
        public ModuleDefinition TargetModule { get; }

        public MethodDefinitionCloner(MethodDefinition sourceMethod, MethodDefinition targetMethod, ModuleDefinition targetModule) {
            SourceMethod = sourceMethod;
            TargetMethod = targetMethod;
            TargetModule = targetModule;

            _instructions = new Instruction[SourceMethod.Body.Instructions.Count];
        }

        public void Clone() {
            // Clone local variables
            foreach (VariableDefinition variable in SourceMethod.Body.Variables) {
                VariableDefinition targetVariable = new VariableDefinition(variable.Name, ImportTypeReference(variable.VariableType));
                TargetMethod.Body.Variables.Add(targetVariable);
            }

            // Clone body instructions
            for (int i = 0; i < SourceMethod.Body.Instructions.Count; i++)
            {
                if (_instructions[i] != null)
                    continue;

                _instructions[i] = CloneInstruction(SourceMethod.Body.Instructions[i]);
            }

            // Insert instructions into target method
            for (int i = 0; i < SourceMethod.Body.Instructions.Count; i++) {
                TargetMethod.Body.Instructions.Add(_instructions[i]);
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

        protected virtual Instruction CloneInstruction(Instruction instruction) {
            Instruction cloneInstruction;
            object operand = instruction.Operand;

            switch (operand) {
                case byte byteOperand:
                    cloneInstruction = CloneByteOperandInstruction(instruction, byteOperand);
                    break;
                case sbyte sbyteOperand:
                    cloneInstruction = CloneSByteOperandInstruction(instruction, sbyteOperand);
                    break;
                case int intOperand:
                    cloneInstruction = CloneIntOperandInstruction(instruction, intOperand);
                    break;
                case long longOperand:
                    cloneInstruction = CloneLongOperandInstruction(instruction, longOperand);
                    break;
                case float floatOperand:
                    cloneInstruction = CloneFloatOperandInstruction(instruction, floatOperand);
                    break;
                case double doubleOperand:
                    cloneInstruction = CloneDoubleOperandInstruction(instruction, doubleOperand);
                    break;
                case string stringOperand:
                    cloneInstruction = CloneStringOperandInstruction(instruction, stringOperand);
                    break;
                case TypeReference typeReferenceOperand:
                    cloneInstruction = CloneTypeReferenceOperandInstruction(instruction, typeReferenceOperand);
                    break;
                case FieldReference fieldReferenceOperand:
                    cloneInstruction = CloneFieldReferenceOperandInstruction(instruction, fieldReferenceOperand);
                    break;
                case MethodReference methodReferenceOperand:
                    cloneInstruction = CloneMethodReferenceOperandInstruction(instruction, methodReferenceOperand);
                    break;
                case ParameterDefinition parameterDefinitionOperand:
                    cloneInstruction = CloneParameterDefinitionOperandInstruction(instruction, parameterDefinitionOperand);
                    break;
                case VariableDefinition variableDefinitionOperand:
                    cloneInstruction = CloneVariableDefinitionOperandInstruction(instruction, variableDefinitionOperand);
                    break;
                case Instruction instructionOperand:
                    cloneInstruction = CloneInstructionOperandInstruction(instruction, instructionOperand);
                    break;
                case Instruction[] instructionArrayOperand:
                    cloneInstruction = CloneInstructionOperandInstructionArray(instruction, instructionArrayOperand);
                    break;
                case null:
                    cloneInstruction = CloneNoOperandInstruction(instruction);
                    break;
                default:
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
            return Instruction.Create(sourceInstruction.OpCode, TargetMethod.Body.Variables[operand.Index]);
        }

        protected virtual Instruction CloneInstructionOperandInstruction(Instruction sourceInstruction, Instruction operand) {
            return Instruction.Create(sourceInstruction.OpCode, GetCopiedInstruction(operand));
        }

        private Instruction CloneInstructionOperandInstructionArray(Instruction sourceInstruction, Instruction[] operand) {
            Instruction[] cloneOperand = new Instruction[operand.Length];
            for (int i = 0; i < operand.Length; i++) {
                cloneOperand[i] = GetCopiedInstruction(operand[i]);
            }

            return Instruction.Create(sourceInstruction.OpCode, cloneOperand);
        }

        private Instruction GetMatchingInstructionByIndex(Instruction sourceInstruction) {
            if (sourceInstruction == null)
                return null;

            int sourceIndex = SourceMethod.Body.Instructions.IndexOf(sourceInstruction);
            if (sourceIndex == -1)
                throw new MethodInlineInjectorException("Matching instruction not found in source method");

            return TargetMethod.Body.Instructions[sourceIndex];
        }

        private Instruction GetCopiedInstruction(Instruction sourceInstruction) {
            if (sourceInstruction == null)
                return null;

            int instructionIndex = SourceMethod.Body.Instructions.IndexOf(sourceInstruction);
            if (instructionIndex == -1)
                throw new MethodInlineInjectorException($"Source instruction not found");

            if (_instructions[instructionIndex] == null) {
                _instructions[instructionIndex] = CloneInstruction(sourceInstruction);
            }

            return _instructions[instructionIndex];
        }
    }
}
