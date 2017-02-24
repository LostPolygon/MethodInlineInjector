using System;
using System.Collections.Generic;
using System.Xml;

namespace LostPolygon.AssemblyMethodInjector {
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