using System;
using System.Collections.Generic;
using System.Xml;

namespace LostPolygon.AssemblyMethodInlineInjector {
    internal static class SimpleXmlSerializableExtensions {
        public static void ProcessCollection<T>(this SimpleXmlSerializable simpleXmlSerializable, ICollection<T> collection, Func<T> createFunc = null) where T : class, ISimpleXmlSerializable {
            if (simpleXmlSerializable.SerializationHelper.IsXmlSerializationReading) {
                while (simpleXmlSerializable.SerializationHelper.XmlSerializationReader.NodeType != XmlNodeType.EndElement) {
                    T value = createFunc?.Invoke() ?? Activator.CreateInstance<T>();
                    value.SerializeWithInheritedMode(simpleXmlSerializable);
                    collection.Add(value);
                }
            } else {
                foreach (T value in collection) {
                    value.SerializeWithInheritedMode(simpleXmlSerializable);
                }
            }
        }
    }
}