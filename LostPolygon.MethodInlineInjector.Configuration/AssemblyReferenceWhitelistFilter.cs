using System;
using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Assembly")]
    public class AssemblyReferenceWhitelistFilter : SimpleXmlSerializable, IAssemblyReferenceWhitelistItem {
        public string Name { get; private set; }
        public bool IsStrictNameCheck { get; private set; }

        private AssemblyReferenceWhitelistFilter() {
        }

        public AssemblyReferenceWhitelistFilter(string name, bool isStrictNameCheck) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsStrictNameCheck = isStrictNameCheck;
        }

        public override string ToString() {
            return $"{nameof(Name)}: '{Name}', {nameof(IsStrictNameCheck)}: {IsStrictNameCheck}";
        }

        #region With.Fody

        public AssemblyReferenceWhitelistFilter WithName(string value) => null;
        public AssemblyReferenceWhitelistFilter WithIsStrictNameCheck(bool value) => null;

        #endregion

        #region ISimpleXmlSerializable

        void ISimpleXmlSerializable.Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            {
                SerializationHelper.ProcessAttributeString(nameof(Name), s => Name = s, () => Name);
                SerializationHelper.ProcessAttributeString(nameof(IsStrictNameCheck), s => IsStrictNameCheck = Convert.ToBoolean(s), () => Convert.ToString(IsStrictNameCheck));
            }
            SerializationHelper.ProcessAdvanceOnRead();
            SerializationHelper.ProcessEndElement();
        }

        #endregion
    }
}