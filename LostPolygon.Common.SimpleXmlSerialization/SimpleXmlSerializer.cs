using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml;

namespace LostPolygon.Common.SimpleXmlSerialization {
    public class SimpleXmlSerializer : SimpleXmlSerializerBase {
        public SimpleXmlSerializer(bool isDeserealizing, XmlDocument xmlDocument, XmlElement currentXmlElement)
            : base(isDeserealizing, xmlDocument, currentXmlElement) {
        }

        protected override SimpleXmlSerializerBase Clone() {
            return new SimpleXmlSerializer(IsDeserializing, Document, CurrentXmlElement);
        }

        public override bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc) {
            if (IsDeserializing) {
                if (!CurrentXmlElement.HasAttribute(name))
                    return false;

                string xmlValue = CurrentXmlElement.GetAttribute(name);
                readAction(xmlValue);
            } else {
                string xmlValue = writeFunc();
                CurrentXmlElement.SetAttribute(name, xmlValue);
            }

            return true;
        }

        public override bool ProcessStartElement(string name, string prefix = null, string namespaceUri = null) {
            if (IsDeserializing) {
                return CurrentXmlElement.Name == name;
            } else {
                XmlElement newElement;
                if (prefix == null || namespaceUri == null) {
                    newElement = Document.CreateElement(name);
                } else {
                    newElement = Document.CreateElement(prefix, name, namespaceUri);
                }

                if (CurrentXmlElement == null) {
                    Document.InsertBefore(newElement, null);
                } else {
                    CurrentXmlElement.AppendChild(newElement);
                }
                CurrentXmlElement = newElement;

                return true;
            }
        }

        public override void ProcessEndElement() {
            if (CurrentXmlElement.ParentNode == null || CurrentXmlElement.ParentNode is XmlDocument) {
                CurrentXmlElement = null;
            } else {
                if (IsDeserializing) {
                    if (CurrentXmlElement.NextSibling != null) {
                        CurrentXmlElement = (XmlElement) CurrentXmlElement.NextSibling;
                    } else {
                        CurrentXmlElement = (XmlElement) CurrentXmlElement.ParentNode;
                    }
                } else {
                    CurrentXmlElement = (XmlElement) CurrentXmlElement.ParentNode;
                }
            }
        }

        public override void ProcessAdvanceOnRead() {
            if (IsDeserializing && CurrentXmlElement.HasChildNodes) {
                CurrentXmlElement = (XmlElement) CurrentXmlElement.FirstChild;
            }
        }

        public override void ProcessCollection<T>(
            ICollection<T> collection,
            Func<SimpleXmlSerializerBase, T> createItemFunc = null) {
            if (IsDeserializing) {
                XmlElement startElement = (XmlElement) CurrentXmlElement.ParentNode;
                XmlElement prevElement;
                do {
                    prevElement = CurrentXmlElement;

                    SimpleXmlSerializerBase clonedSerializer = Clone();
                    T value = createItemFunc?.Invoke(clonedSerializer) ?? InvokeSerializationMethod<T>(null, clonedSerializer);

                    CurrentXmlElement = clonedSerializer.CurrentXmlElement;
                    collection.Add(value);
                    if (clonedSerializer.CurrentXmlElement == startElement)
                        break;
                } while (prevElement != CurrentXmlElement || CurrentXmlElement.NextSibling != null);
            } else {
                foreach (T value in collection) {
                    CloneSerializerAndInvokeSerializationMethod(value);
                }
            }
        }

        public override void ProcessCollectionAsReadOnly<T>(
            Action<ReadOnlyCollection<T>> collectionSetAction,
            Func<ReadOnlyCollection<T>> collectionGetFunc,
            Func<SimpleXmlSerializerBase, T> createItemFunc = null) {
            if (IsDeserializing) {
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

            if (IsDeserializing) {
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

            if (IsDeserializing) {
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
            if (IsDeserializing) {
                do {
                    action();
                } while (CurrentXmlElement.NextSibling != null);
            } else {
                action();
            }
        }
    }
}