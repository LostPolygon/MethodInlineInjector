using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInjector {
    public abstract class SimpleXmlSerializable : IXmlSerializable, ISimpleXmlSerializable {
        private readonly XmlSerializationHelper _serializationHelper;

        internal XmlSerializationHelper SerializationHelper => _serializationHelper;

        public SimpleXmlSerializable() {
            _serializationHelper = new XmlSerializationHelper(Serialize);
        }

        public XmlSchema GetSchema() {
            throw new NotImplementedException();
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
                throw new ArgumentException();
            }
        }

        public void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable) {
            _serializationHelper.SerializeWithInheritedMode(
                simpleXmlSerializable._serializationHelper.XmlSerializationReader,
                simpleXmlSerializable._serializationHelper.XmlSerializationWriter);
        }

        protected void ProcessCollection<T>(ICollection<T> collection, Func<T> createFunc = null) where T : class, ISimpleXmlSerializable {
            if (_serializationHelper.IsXmlSerializationReading) {
                while (_serializationHelper.XmlSerializationReader.NodeType != XmlNodeType.EndElement) {
                    T value = createFunc?.Invoke() ?? Activator.CreateInstance<T>();
                    value.SerializeWithInheritedMode(this);
                    collection.Add(value);
                }
            } else {
                foreach (T value in collection) {
                    value.SerializeWithInheritedMode(this);
                }
            }
        }
    }
}