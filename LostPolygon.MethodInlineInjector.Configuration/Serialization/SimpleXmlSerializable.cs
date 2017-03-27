using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializable : ISimpleXmlSerializable {
        private readonly SimpleXmlSerializer _serializer;

        protected SimpleXmlSerializer Serializer => _serializer;

        protected SimpleXmlSerializable() {
            _serializer = new SimpleXmlSerializer(this);
        }

        #region ISimpleXmlSerializable

        protected virtual void Serialize() {
            if (_serializer.XmlSerializationReader == null && _serializer.XmlSerializationWriter == null ||
                _serializer.XmlSerializationReader != null && _serializer.XmlSerializationWriter != null) {
                throw new InvalidOperationException();
            }
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
            _serializer.ReadXml(reader);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) {
            _serializer.WriteXml(writer);
        }

        #endregion
    }
}