using System.Collections.ObjectModel;

namespace LostPolygon.MethodInlineInjector {
    internal static class ReadOnlyCollectionUtility<T> {
        public static readonly ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(new T[0]);
    }
}
