using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Configuration {
    public static class SimpleXmlSerializationUtility {
        private static readonly XmlSerializerNamespaces kEmptyXmlSerializerNamespaces;

        static SimpleXmlSerializationUtility() {
            kEmptyXmlSerializerNamespaces = new XmlSerializerNamespaces();
            kEmptyXmlSerializerNamespaces.Add("", "");
        }

        public static string XmlSerializeToString<T>(T objectInstance, Encoding encoding = null) where T : ISimpleXmlSerializable {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }

            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlWriter xmlWriter = new XmlTextWriter(textWriter)) {
                    objectInstance.WriteXml(xmlWriter);
                }
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData) where T : ISimpleXmlSerializable {
            T result;
            using (TextReader textReader = new StringReader(objectData)) {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings {
                    IgnoreWhitespace = true,
                    IgnoreComments = true
                };
                XmlReader xmlReader = XmlReader.Create(textReader, xmlReaderSettings);
                while (xmlReader.NodeType != XmlNodeType.Element) {
                    xmlReader.Read();
                }

                result = Activator.CreateInstance<T>();
                result.ReadXml(xmlReader);
            }

            return result;
        }
    }
}