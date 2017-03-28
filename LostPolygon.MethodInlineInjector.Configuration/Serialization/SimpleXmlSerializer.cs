using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public class SimpleXmlSerializer : SimpleXmlSerializerBase {
        private readonly Stack<String> _readStartedElementNamesStack = new Stack<string>();

        public SimpleXmlSerializer(ISimpleXmlSerializable simpleXmlSerializable) : base(simpleXmlSerializable) {
        }

        protected override SimpleXmlSerializerBase Clone(ISimpleXmlSerializable simpleXmlSerializable) {
            return new SimpleXmlSerializer(simpleXmlSerializable);
        }

        public override bool ProcessElementString(string name, Action<string> readAction, Func<string> writeFunc) {
            if (IsXmlSerializationReading) {
                string xmlValue = XmlSerializationReader.ReadElementString(name);
                if (xmlValue == null)
                    return false;

                readAction(xmlValue);
            } else {
                string xmlValue = writeFunc();
                XmlSerializationWriter.WriteElementString(name, xmlValue);
            }

            return true;
        }

        public override bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc) {
            if (IsXmlSerializationReading) {
                string xmlValue = XmlSerializationReader.GetAttribute(name);
                if (xmlValue == null)
                    return false;

                readAction(xmlValue);
            } else {
                string xmlValue = writeFunc();
                XmlSerializationWriter.WriteAttributeString(name, xmlValue);
            }

            return true;
        }

        public override bool ProcessStartElement(string name) {
            if (IsXmlSerializationReading) {
                if (XmlSerializationReader.MoveToContent() != XmlNodeType.Element)
                    return false;

                if (XmlSerializationReader.Name != name)
                    return false;

                _readStartedElementNamesStack.Push(name);
            } else {
                XmlSerializationWriter.WriteStartElement(name);
            }

            return true;
        }

        public override void ProcessEndElement(bool readEndElement = true) {
            if (IsXmlSerializationReading) {
                string lastStartedElementName = _readStartedElementNamesStack.Pop();
                if (readEndElement && XmlSerializationReader.MoveToContent() == XmlNodeType.EndElement && lastStartedElementName == XmlSerializationReader.Name) {
                    XmlSerializationReader.ReadEndElement();
                }
            } else {
                XmlSerializationWriter.WriteEndElement();
            }
        }

        public override void ProcessAdvanceOnRead() {
            if (IsXmlSerializationReading) {
                XmlSerializationReader.Read();
            }
        }

        public override void ProcessCollection<T>(
            ICollection<T> collection,
            Func<T> createItemFunc = null) {
            if (IsXmlSerializationReading) {
                while (XmlSerializationReader.NodeType != XmlNodeType.EndElement) {
                    T value = createItemFunc?.Invoke() ?? (T) Activator.CreateInstance(typeof(T), true);
                    CloneAndAssignSerializer(value);
                    value.ReadXml(XmlSerializationReader);
                    collection.Add(value);
                }
            } else {
                foreach (T value in collection) {
                    CloneAndAssignSerializer(value);
                    value.WriteXml(XmlSerializationWriter);
                }
            }
        }

        public override void ProcessCollectionAsReadOnly<T>(
            Action<ReadOnlyCollection<T>> collectionSetAction,
            Func<ReadOnlyCollection<T>> collectionGetFunc,
            Func<T> createItemFunc = null) {
            if (IsXmlSerializationReading) {
                List<T> list = new List<T>();
                ProcessCollection(list, createItemFunc);
                collectionSetAction(list.AsReadOnly());
            } else {
                ProcessCollection(collectionGetFunc(), createItemFunc);
            }
        }

        public override bool ProcessEnumAttribute<T>(string name, Action<T> readAction, Func<T> writeFunc) {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new SerializationException("value must be an Enum");

            if (IsXmlSerializationReading) {
                string stringEnumValue = null;
                if (!ProcessAttributeString(name, s => stringEnumValue = s, null))
                    return false;

                T parsedEnumValue = (T) Enum.Parse(enumType, stringEnumValue, true);
                readAction(parsedEnumValue);
            } else {
                T enumValue = writeFunc();
                ProcessAttributeString(name, null, () => Enum.GetName(enumType, enumValue));
            }

            return true;
        }

        public override void ProcessFlagsEnumAttributes<T>(T defaultValue, Action<T> readAction, Func<T> writeFunc) {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new SerializationException("value must be an Enum");

            long defaultValueLong = defaultValue.ToInt64(null);
            T[] enumValues = (T[]) Enum.GetValues(enumType);
            string[] enumNames = Enum.GetNames(enumType);

            if (IsXmlSerializationReading) {
                long resultEnumValue = defaultValueLong;
                for (int i = 0; i < enumNames.Length; i++) {
                    string flagName = enumNames[i];
                    long flagValue = enumValues[i].ToInt64(null);

                    bool currentFlagValue = false;
                    if (ProcessAttributeString(flagName, s => currentFlagValue = Convert.ToBoolean(s), null)) {
                        if (currentFlagValue) {
                            resultEnumValue |= flagValue;
                        } else {
                            resultEnumValue &= ~flagValue;
                        }
                    }
                }

                T enumValue = (T) Enum.ToObject(enumType, resultEnumValue);
                readAction(enumValue);
            } else {
                long currentEnumValue = writeFunc().ToInt64(null);
                for (int i = 0; i < enumNames.Length; i++) {
                    string flagName = enumNames[i];
                    long flagValue = enumValues[i].ToInt64(null);

                    long currentFlag = currentEnumValue & flagValue;
                    ProcessAttributeString(flagName, null, () => Convert.ToString(currentFlag != 0));
                }
            }
        }

        public override void ProcessWhileNotElementEnd(Action action) {
            if (IsXmlSerializationReading) {
                while (XmlSerializationReader.NodeType != XmlNodeType.EndElement &&
                       XmlSerializationReader.NodeType != XmlNodeType.EndEntity) {
                    action();
                }
            } else {
                action();
            }
        }
    }
}