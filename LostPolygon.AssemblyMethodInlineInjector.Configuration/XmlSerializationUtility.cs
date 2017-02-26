using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInlineInjector.Configuration {
    public static class XmlSerializationUtility {
        private static readonly XmlSerializerNamespaces kEmptyXmlSerializerNamespaces;

        static XmlSerializationUtility() {
            kEmptyXmlSerializerNamespaces = new XmlSerializerNamespaces();
            kEmptyXmlSerializerNamespaces.Add("", "");
        }

        public static string XmlSerializeToString(object objectInstance, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }

            XmlSerializer serializer = new XmlSerializer(objectInstance.GetType());
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = new XmlTextWriter(new MemoryStream(), encoding)) {
                serializer.Serialize(writer, objectInstance, kEmptyXmlSerializerNamespaces);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData) {
            return (T) XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(string objectData, Type type) {
            XmlSerializer serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData)) {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }
}