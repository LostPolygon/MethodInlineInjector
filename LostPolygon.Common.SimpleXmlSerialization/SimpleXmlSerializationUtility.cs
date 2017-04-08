using System.IO;
using System.Text;
using System.Xml;

namespace LostPolygon.Common.SimpleXmlSerialization {
    public static class SimpleXmlSerializationUtility {
        public static string GenerateXmlSchemaString<T>(T serializedObject) where T : class {
            if (serializedObject == null)
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

                    SchemaGeneratorSimpleXmlSerializer serializer = new SchemaGeneratorSimpleXmlSerializer(xmlDocument, schemaElement);
                    SimpleXmlSerializerBase.InvokeSerializationMethod(serializedObject, serializer);

                    serializer.InsertCapturedTypes();

                    xmlDocument.WriteContentTo(xmlTextWriter);
                }
            }

            return sb.ToString();
        }

        public static string XmlSerializeToString<T>(T serializedObject) where T : class {
            if (serializedObject == null)
                return "";

            XmlDocument xmlDocument = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb)) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter)) {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.IndentChar = ' ';
                    xmlTextWriter.Indentation = 4;
                    xmlTextWriter.Namespaces = false;

                    SimpleXmlSerializer serializer = new SimpleXmlSerializer(false, xmlDocument, xmlDocument.DocumentElement);
                    SimpleXmlSerializerBase.InvokeSerializationMethod(serializedObject, serializer);

                    xmlDocument.WriteContentTo(xmlTextWriter);
                }
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData) where T : class {
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

                result = SimpleXmlSerializerBase.InvokeSerializationMethod<T>(null, new SimpleXmlSerializer(true, xmlDocument, xmlDocument.DocumentElement));
            }

            return result;
        }
    }
}