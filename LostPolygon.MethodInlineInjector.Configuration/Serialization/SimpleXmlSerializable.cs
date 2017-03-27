using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializable : ISimpleXmlSerializable {
        protected SimpleXmlSerializerBase Serializer { get; private set; }

        #region ISimpleXmlSerializable

        protected virtual void Serialize() {
            if (Serializer.XmlSerializationReader == null && Serializer.XmlSerializationWriter == null ||
                Serializer.XmlSerializationReader != null && Serializer.XmlSerializationWriter != null) {
                throw new InvalidOperationException();
            }
        }

        void ISimpleXmlSerializable.SetSerializer(SimpleXmlSerializerBase serializer) {
            Serializer = serializer;
        }

        void ISimpleXmlSerializable.Serialize() {
            Serialize();
        }

        #endregion

        #region IXmlSerializable

        XmlSchema IXmlSerializable.GetSchema() {
            throw new InvalidOperationException();
        }

        void IXmlSerializable.ReadXml(XmlReader reader) {
            Serializer.ReadXml(reader);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) {
            Serializer.WriteXml(writer);
        }

        #endregion
    }
}