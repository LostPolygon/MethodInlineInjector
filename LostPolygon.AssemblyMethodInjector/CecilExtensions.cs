using System.Collections.Generic;
using Mono.Collections.Generic;

namespace LostPolygon.AssemblyMethodInjector {
    internal static class CecilExtensions {
        public static void InsertRangeToStart<TResult>(this Collection<TResult> collection, IEnumerable<TResult> source) {
            int inserted = 0;
            foreach (TResult result in source) {
                collection.Insert(inserted, result);
                inserted++;
            }
        }
    }
}