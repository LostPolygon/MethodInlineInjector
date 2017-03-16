using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializable : ISimpleXmlSerializable {
        private readonly SimpleXmlSerializationHelper _serializationHelper;

        protected internal SimpleXmlSerializationHelper SerializationHelper => _serializationHelper;

        protected SimpleXmlSerializable() {
            _serializationHelper = new SimpleXmlSerializationHelper(((ISimpleXmlSerializable) this).Serialize);
        }

        #region ISimpleXmlSerializable

        protected virtual void Serialize() {
            if (_serializationHelper.XmlSerializationReader == null && _serializationHelper.XmlSerializationWriter == null ||
                _serializationHelper.XmlSerializationReader != null && _serializationHelper.XmlSerializationWriter != null) {
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
            _serializationHelper.ReadXml(reader);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) {
            _serializationHelper.WriteXml(writer);
        }

        #endregion
    }
}