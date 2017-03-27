using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializerBase {
        public ISimpleXmlSerializable SimpleXmlSerializable { get; }
        public XmlReader XmlSerializationReader { get; protected set; }
        public XmlWriter XmlSerializationWriter { get; protected set; }
        public bool IsXmlSerializationReading => XmlSerializationReader != null;

        protected SimpleXmlSerializerBase(ISimpleXmlSerializable simpleXmlSerializable) {
            SimpleXmlSerializable = simpleXmlSerializable;
        }

        public abstract SimpleXmlSerializer Clone(ISimpleXmlSerializable simpleXmlSerializable);
        public abstract void ReadXml(XmlReader reader);
        public abstract void WriteXml(XmlWriter writer);
        public abstract bool ProcessElementString(string name, Action<string> readAction, Func<string> writeFunc);
        public abstract bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc);
        public abstract bool ProcessStartElement(string name);
        public abstract void ProcessEndElement(bool readEndElement = true);
        public abstract void ProcessAdvanceOnRead();

        public abstract void ProcessCollection<T>(
            ICollection<T> collection,
            Func<T> createItemFunc = null)
            where T : class, ISimpleXmlSerializable;

        public abstract void ProcessCollectionAsReadOnly<T>(
            Action<ReadOnlyCollection<T>> collectionSetAction,
            Func<ReadOnlyCollection<T>> collectionGetFunc,
            Func<T> createItemFunc = null)
            where T : class, ISimpleXmlSerializable;

        public abstract bool ProcessEnumAttribute<T>(string name, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible;

        public abstract void ProcessFlagsEnumAttributes<T>(T defaultValue, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible;

        public abstract void ProcessWhileNotElementEnd(Action action);
        public abstract T CreateByXmlRootName<T>(string name, params Type[] types) where T : ISimpleXmlSerializable;
        public abstract T CreateByKnownInheritors<T>(string name) where T : ISimpleXmlSerializable;

        protected void CloneAndAssignSerializer(ISimpleXmlSerializable simpleXmlSerializable) {
            simpleXmlSerializable.SetSerializer(Clone(simpleXmlSerializable));
        }
    }
}