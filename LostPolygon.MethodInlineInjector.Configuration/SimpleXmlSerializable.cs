using System;
using System.Xml;
using System.Xml.Schema;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializable : ISimpleXmlSerializable {
        private readonly SimpleXmlSerializationHelper _serializationHelper;

        protected internal SimpleXmlSerializationHelper SerializationHelper => _serializationHelper;

        public SimpleXmlSerializable() {
            _serializationHelper = new SimpleXmlSerializationHelper(Serialize);
        }

        public XmlSchema GetSchema() {
            throw new InvalidOperationException();
        }

        public virtual void ReadXml(XmlReader reader) {
            _serializationHelper.ReadXml(reader);
        }

        public virtual void WriteXml(XmlWriter writer) {
            _serializationHelper.WriteXml(writer);
        }

        public virtual void Serialize() {
            if (_serializationHelper.XmlSerializationReader == null && _serializationHelper.XmlSerializationWriter == null ||
                _serializationHelper.XmlSerializationReader != null && _serializationHelper.XmlSerializationWriter != null) {
                throw new InvalidOperationException();
            }
        }

        public void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable) {
            _serializationHelper.SerializeWithInheritedMode(
                simpleXmlSerializable._serializationHelper.XmlSerializationReader,
                simpleXmlSerializable._serializationHelper.XmlSerializationWriter);
        }
    }
}