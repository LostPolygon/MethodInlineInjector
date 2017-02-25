using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml;

namespace LostPolygon.AssemblyMethodInlineInjector {
    internal class XmlSerializationHelper {
        private readonly Action _serializeAction;
        private readonly Stack<String> _readStartedElementNamesStack = new Stack<string>();

        public XmlReader XmlSerializationReader { get; private set; }

        public XmlWriter XmlSerializationWriter { get; private set; }

        public bool IsXmlSerializationReading {
            get {
                return XmlSerializationReader != null;
            }
        }

        public XmlSerializationHelper(Action serializeAction) {
            _serializeAction = serializeAction;
        }

        public void ReadXml(XmlReader reader) {
            try {
                XmlSerializationWriter = null;
                XmlSerializationReader = reader;
                _serializeAction();
            } finally {
                XmlSerializationReader = null;
            }
        }

        public void WriteXml(XmlWriter writer) {
            try {
                XmlSerializationReader = null;
                XmlSerializationWriter = writer;
                _serializeAction();
            } finally {
                XmlSerializationWriter = null;
            }
        }

        public void SerializeWithInheritedMode(XmlReader reader, XmlWriter writer) {
            try {
                XmlSerializationReader = reader;
                XmlSerializationWriter = writer;
                _serializeAction();
            } finally {
                XmlSerializationReader = null;
                XmlSerializationWriter = null;
            }
        }

        public bool ProcessElementString(string name, Action<string> readAction, Func<string> writeFunc) {
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

        public bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc) {
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

        public bool ProcessStartElement(string name) {
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

        public void ProcessEndElement(bool readEndElement = true) {
            if (IsXmlSerializationReading) {
                string lastStartedElementName = _readStartedElementNamesStack.Pop();
                if (readEndElement && XmlSerializationReader.MoveToContent() == XmlNodeType.EndElement && lastStartedElementName == XmlSerializationReader.Name) {
                    XmlSerializationReader.ReadEndElement();
                }
            } else {
                XmlSerializationWriter.WriteEndElement();
            }
        }

        public void ProcessAdvanceOnRead() {
            if (IsXmlSerializationReading) {
                XmlSerializationReader.Read();
            }
        }

        public bool ProcessEnumAttribute<T>(string name, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible {
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

        public void ProcessFlagsEnumAttributes<T>(T defaultValue, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible {
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
                    if (currentFlag != (defaultValueLong & currentEnumValue)) {
                        ProcessAttributeString(flagName, null, () => Convert.ToString(currentFlag != 0));
                    }
                }
            }
        }

        public void ProcessWhileNotElementEnd(Action action) {
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