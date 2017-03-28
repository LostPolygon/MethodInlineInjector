using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public class SchemaGeneratorSimpleXmlSerializer : SimpleXmlSerializerBase {
        private const string kXmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
        private bool _isOptional = false;
        private bool _isTypeWritten = false;

        public SchemaGeneratorSimpleXmlSerializer(ISimpleXmlSerializable simpleXmlSerializable) : base(simpleXmlSerializable) {
        }

        protected override SimpleXmlSerializerBase Clone(ISimpleXmlSerializable simpleXmlSerializable) {
            return new SchemaGeneratorSimpleXmlSerializer(simpleXmlSerializable);
        }

        public override bool ProcessElementString(string name, Action<string> readAction, Func<string> writeFunc) {
            string xmlValue = writeFunc();
            XmlSerializationWriter.WriteElementString(name, xmlValue);

            return true;
        }

        public override bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc) {
            XmlSerializationWriter.WriteStartElement("xs", "attribute", kXmlSchemaNamespace);
            {
                XmlSerializationWriter.WriteAttributeString("name", name);
                XmlSerializationWriter.WriteAttributeString("type", "xs:string");
                if (!_isOptional) {
                    XmlSerializationWriter.WriteAttributeString("use", "required");
                }
            }
            XmlSerializationWriter.WriteEndElement();

            return true;
        }

        public override bool ProcessStartElement(string name) {
            XmlSerializationWriter.WriteStartElement("xs", "element", kXmlSchemaNamespace);
            XmlSerializationWriter.WriteAttributeString("name", name);
            if (_isOptional) {
                XmlSerializationWriter.WriteAttributeString("minOccurs", "0");
            }
            XmlSerializationWriter.WriteStartElement("xs", "complexType", kXmlSchemaNamespace);
            if (!_isTypeWritten) {
                XmlSerializationWriter.WriteAttributeString("type", SimpleXmlSerializable.GetType().Name);
                _isTypeWritten = true;
            }

            return true;
        }

        public override void ProcessEndElement(bool readEndElement = true) {
            XmlSerializationWriter.WriteEndElement();
            XmlSerializationWriter.WriteEndElement();
        }

        public override void ProcessAdvanceOnRead() {
        }

        public override void ProcessCollection<T>(
            ICollection<T> collection,
            Func<T> createItemFunc = null) {
            XmlSerializationWriter.WriteStartElement("xs", "sequence", kXmlSchemaNamespace);
            {
                T value = collection.FirstOrDefault() ?? throw new InvalidOperationException();
                CloneAndAssignSerializer(value);
                value.WriteXml(XmlSerializationWriter);
            }
            XmlSerializationWriter.WriteEndElement();
        }

        public override void ProcessCollectionAsReadOnly<T>(
            Action<ReadOnlyCollection<T>> collectionSetAction,
            Func<ReadOnlyCollection<T>> collectionGetFunc,
            Func<T> createItemFunc = null) {
            ProcessCollection(collectionGetFunc(), createItemFunc);
        }

        public override bool ProcessEnumAttribute<T>(string name, Action<T> readAction, Func<T> writeFunc) {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new SerializationException("value must be an Enum");

            XmlSerializationWriter.WriteStartElement("xs", "attribute", kXmlSchemaNamespace);
            XmlSerializationWriter.WriteAttributeString("name", name);
            if (!_isOptional) {
                XmlSerializationWriter.WriteAttributeString("use", "required");
            }
            {
                XmlSerializationWriter.WriteStartElement("xs", "simpleType", kXmlSchemaNamespace);
                {
                    XmlSerializationWriter.WriteStartElement("xs", "restriction", kXmlSchemaNamespace);
                    XmlSerializationWriter.WriteAttributeString("base", "xs:string");
                    {
                        string[] enumNames = Enum.GetNames(enumType);
                        for (int i = 0; i < enumNames.Length; i++) {
                            string enumName = enumNames[i];
                            XmlSerializationWriter.WriteStartElement("xs", "enumeration", kXmlSchemaNamespace);
                            {
                                XmlSerializationWriter.WriteAttributeString("value", enumName);
                            }
                            XmlSerializationWriter.WriteEndElement();
                        }
                    }
                    XmlSerializationWriter.WriteEndElement();
                }
                XmlSerializationWriter.WriteEndElement();
            }
            XmlSerializationWriter.WriteEndElement();


            return true;
        }

        public override void ProcessFlagsEnumAttributes<T>(T defaultValue, Action<T> readAction, Func<T> writeFunc) {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new SerializationException("value must be an Enum");

            T[] enumValues = (T[]) Enum.GetValues(enumType);
            string[] enumNames = Enum.GetNames(enumType);

            long currentEnumValue = writeFunc().ToInt64(null);
            for (int i = 0; i < enumNames.Length; i++) {
                string flagName = enumNames[i];
                long flagValue = enumValues[i].ToInt64(null);

                long currentFlag = currentEnumValue & flagValue;
                ProcessAttributeString(flagName, null, () => Convert.ToString(currentFlag != 0));
            }
        }

        public override void ProcessWhileNotElementEnd(Action action) {
            ProcessOptional(action);
        }

        public override void ProcessOptional(Action action) {
            _isOptional = true;
            action();
            _isOptional = false;
        }
    }
}
