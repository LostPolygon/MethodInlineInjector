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
            Name = name;
            IsStrictNameCheck = isStrictNameCheck;
        }

        public override string ToString() {
            return $"{nameof(Name)}: '{Name}', {nameof(IsStrictNameCheck)}: {IsStrictNameCheck}";
        }

        #region Serialization

        public override void Serialize() {
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