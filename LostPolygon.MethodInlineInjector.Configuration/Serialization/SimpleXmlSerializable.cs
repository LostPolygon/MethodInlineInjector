using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializable : ISimpleXmlSerializable {
        #region ISimpleXmlSerializable

        protected virtual void Serialize() {

        }

        protected SimpleXmlSerializerBase Serializer { get; set; }

        SimpleXmlSerializerBase ISimpleXmlSerializable.Serializer {
            get => Serializer;
            set => Serializer = value;
        }

        [DebuggerStepThrough]
        void ISimpleXmlSerializable.Serialize() {
            Serialize();
        }

        #endregion
    }
}