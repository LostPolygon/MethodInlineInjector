using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace LostPolygon.MethodInlineInjector {
    internal static class CecilExtensions {
        public static void InsertRangeToStart<TResult>(this Collection<TResult> collection, IEnumerable<TResult> source) {
            int inserted = 0;
            foreach (TResult result in source) {
                collection.Insert(inserted, result);
                inserted++;
            }
        }

        public static Instruction ReplaceAndFixReferences(this ILProcessor processor, Instruction oldInstruction, Instruction newInstruction) {
            processor.Replace(oldInstruction, newInstruction);
            foreach (Instruction bodyInstruction in processor.Body.Instructions) {
                ReplaceOperandInstruction(bodyInstruction, oldInstruction, newInstruction);
            }

            foreach (ExceptionHandler exceptionHandler in processor.Body.ExceptionHandlers) {
                ReplaceOperandInstruction(exceptionHandler.FilterStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.HandlerStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.HandlerEnd, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.TryStart, oldInstruction, newInstruction);
                ReplaceOperandInstruction(exceptionHandler.TryEnd, oldInstruction, newInstruction);
            }

            return newInstruction;
        }

        private static void ReplaceOperandInstruction(Instruction bodyInstruction, Instruction oldInstruction, Instruction newInstruction) {
            if (bodyInstruction == null)
                return;

            if (bodyInstruction.Operand == oldInstruction) {
                bodyInstruction.Operand = newInstruction;
            }
        }

        public static TSource Last<TSource>(this Collection<TSource> source, Func<TSource, bool> predicate) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = source.Count - 1; i >= 0; i--) {
                TSource item = source[i];
                if (predicate(item))
                    return item;
            }

            throw new InvalidOperationException("No matching element found");
        }

        public static TSource Last<TSource>(this Collection<TSource> source) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new InvalidOperationException("Empty collection");

            return source[source.Count - 1];
        }
    }
}