using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace LostPolygon.AssemblyMethodInlineInjector {
    internal static class CecilExtensions {
        public static void InsertRangeToStart<TResult>(this Collection<TResult> collection, IEnumerable<TResult> source) {
            int inserted = 0;
            foreach (TResult result in source) {
                collection.Insert(inserted, result);
                inserted++;
            }
        }

        public static void ReplaceAndFixReferences(this ILProcessor processor, Instruction oldInstruction, Instruction newInstruction, MethodDefinition methodDefinition) {
            processor.Replace(oldInstruction, newInstruction);
            foreach (Instruction bodyInstruction in methodDefinition.Body.Instructions) {
                ReplaceOperandInstruction(bodyInstruction, oldInstruction, newInstruction);
            }

            foreach (ExceptionHandler exceptionHandler in methodDefinition.Body.ExceptionHandlers) {
                ReplaceOperandInstruction(exceptionHandler.FilterStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.HandlerStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.HandlerEnd, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.TryStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.TryEnd, oldInstruction, newInstruction);
            }
        }

        private static void ReplaceOperandInstruction(Instruction bodyInstruction, Instruction oldInstruction, Instruction newInstruction) {
            if (bodyInstruction == null)
                return;

            if (bodyInstruction.Operand == oldInstruction) {
                bodyInstruction.Operand = newInstruction;
            }
        }
    }
}