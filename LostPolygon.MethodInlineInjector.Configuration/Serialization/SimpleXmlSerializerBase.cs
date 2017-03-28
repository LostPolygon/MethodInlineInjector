using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializerBase {
        public ISimpleXmlSerializable SimpleXmlSerializable { get; }
        public XmlReader XmlSerializationReader { get; protected set; }
        public XmlWriter XmlSerializationWriter { get; protected set; }
        public bool IsXmlSerializationReading => XmlSerializationReader != null;

        protected SimpleXmlSerializerBase(ISimpleXmlSerializable simpleXmlSerializable) {
            SimpleXmlSerializable = simpleXmlSerializable;
        }

        protected abstract SimpleXmlSerializerBase Clone(ISimpleXmlSerializable simpleXmlSerializable);

        public virtual void ReadXml(XmlReader reader) {
            try {
                XmlSerializationWriter = null;
                XmlSerializationReader = reader;
                SimpleXmlSerializable.Serialize();
            } finally {
                XmlSerializationReader = null;
            }
        }

        public virtual void WriteXml(XmlWriter writer) {
            try {
                XmlSerializationReader = null;
                XmlSerializationWriter = writer;
                SimpleXmlSerializable.Serialize();
            } finally {
                XmlSerializationWriter = null;
            }
        }

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

        public virtual void ProcessOptional(Action action) {
            action();
        }

        protected void CloneAndAssignSerializer(ISimpleXmlSerializable simpleXmlSerializable) {
            simpleXmlSerializable.SetSerializer(Clone(simpleXmlSerializable));
        }

        public virtual T CreateByXmlRootName<T>(string name, params Type[] types)
            where T : class, ISimpleXmlSerializable {
            foreach (Type type in types) {
                if (GetXmlRootName(type) == name) {
                    T value = (T) Activator.CreateInstance(type, true);
                    CloneAndAssignSerializer(value);
                    return value;
                }
            }

            throw new NotSupportedException($"Unknown element name {name}");
        }

        public virtual T CreateByKnownInheritors<T>(string name)
            where T : class, ISimpleXmlSerializable {
            IEnumerable<Type> knownInheritors =
                Attribute
                    .GetCustomAttributes(typeof(T), typeof(KnownInheritorsAttribute))
                    .Cast<KnownInheritorsAttribute>()
                    .SelectMany(attribute => attribute.InheritorTypes)
                    .Distinct();

            bool isEmpty = true;
            foreach (Type type in knownInheritors) {
                isEmpty = false;
                if (GetXmlRootName(type) == name) {
                    T value = (T) Activator.CreateInstance(type, true);
                    CloneAndAssignSerializer(value);
                    return value;
                }
            }

            if (isEmpty)
                throw new InvalidOperationException($"Type {typeof(T)} has no {nameof(KnownInheritorsAttribute)} attached");

            throw new NotSupportedException($"Unknown element name {name}");
        }

        public virtual string GetXmlRootName(Type type) {
            if (!typeof(ISimpleXmlSerializable).IsAssignableFrom(type))
                throw new Exception($"{nameof(type.Name)} must implement {nameof(ISimpleXmlSerializable)}");

            XmlRootAttribute xmlRootAttribute = (XmlRootAttribute) Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute));
            if (xmlRootAttribute == null)
                throw new Exception($"No {nameof(XmlRootAttribute)} defined on {type.Name}");

            return xmlRootAttribute.ElementName;
        }

        public string GetXmlRootName<T>() where T : ISimpleXmlSerializable {
            return GetXmlRootName(typeof(T));
        }
    }
}