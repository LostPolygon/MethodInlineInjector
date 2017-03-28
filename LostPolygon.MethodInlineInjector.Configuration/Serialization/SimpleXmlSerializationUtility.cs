using System;
using System.IO;
using System.Text;
using System.Xml;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public static class SimpleXmlSerializationUtility {
        public static string GenerateXmlSchemaString<T>(T objectInstance) where T : ISimpleXmlSerializable {
            if (objectInstance == null)
                return "";

            XmlDocument xmlDocument = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.IndentChar = ' ';
                    xmlTextWriter.Indentation = 4;

                    XmlElement schemaElement = xmlDocument.CreateElement("xs", "schema", "http://www.w3.org/2001/XMLSchema");
                    schemaElement.SetAttribute("elementFormDefault", "qualified");
                    schemaElement.SetAttribute("attributeFormDefault", "unqualified");
                    xmlDocument.InsertBefore(schemaElement, null);

                    SchemaGeneratorSimpleXmlSerializer serializer = new SchemaGeneratorSimpleXmlSerializer(objectInstance, xmlDocument, schemaElement);
                    objectInstance.Serializer = serializer;
                    objectInstance.Serialize();

                    serializer.InsertCapturedTypes();

                    xmlDocument.WriteContentTo(xmlTextWriter);
                }
            }

            return sb.ToString();
        }

        public static string XmlSerializeToString<T>(T objectInstance) where T : ISimpleXmlSerializable {
            if (objectInstance == null)
                return "";

            XmlDocument xmlDocument = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.IndentChar = ' ';
                    xmlTextWriter.Indentation = 4;
                    xmlTextWriter.Namespaces = false;
                    objectInstance.Serializer =
                        new SimpleXmlSerializer(false, objectInstance, xmlDocument, xmlDocument.DocumentElement);
                    objectInstance.Serialize();

                    xmlDocument.WriteContentTo(xmlTextWriter);
                }
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData) where T : class, ISimpleXmlSerializable {
            XmlDocument xmlDocument = new XmlDocument();

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
                xmlDocument.Load(xmlReader);

                result = (T) Activator.CreateInstance(typeof(T), true);
                result.Serializer =
                    new SimpleXmlSerializer(true, result, xmlDocument, xmlDocument.DocumentElement);
                result.Serialize();
            }

            return result;
        }
    }
}