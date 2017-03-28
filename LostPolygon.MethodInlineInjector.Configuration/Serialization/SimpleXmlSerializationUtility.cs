using System;
using System.IO;
using System.Text;
using System.Xml;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public static class SimpleXmlSerializationUtility {
        public static string GenerateXmlSchemaString<T>(T objectInstance) where T : ISimpleXmlSerializable {
            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.IndentChar = ' ';
                    xmlTextWriter.Indentation = 4;
                    objectInstance.SetSerializer(new SchemaGeneratorSimpleXmlSerializer(objectInstance));

                    xmlTextWriter.WriteStartElement("xs", "schema", "http://www.w3.org/2001/XMLSchema");
                    xmlTextWriter.WriteAttributeString("elementFormDefault", "qualified");
                    xmlTextWriter.WriteAttributeString("attributeFormDefault", "unqualified");

                    objectInstance.WriteXml(xmlTextWriter);

                    xmlTextWriter.WriteEndElement();
                }
            }

            return sb.ToString();
        }

        public static string XmlSerializeToString<T>(T objectInstance) where T : ISimpleXmlSerializable {
            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.IndentChar = ' ';
                    xmlTextWriter.Indentation = 4;
                    xmlTextWriter.Namespaces = false;
                    objectInstance.SetSerializer(new SimpleXmlSerializer(objectInstance));
                    objectInstance.WriteXml(xmlTextWriter);
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

                result = (T) Activator.CreateInstance(typeof(T), true);
                result.SetSerializer(new SimpleXmlSerializer(result));
                result.ReadXml(xmlReader);
            }

            return result;
        }
    }
}