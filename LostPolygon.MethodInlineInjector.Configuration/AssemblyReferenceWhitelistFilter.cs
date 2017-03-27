using System;
using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Assembly")]
    public class AssemblyReferenceWhitelistFilter : SimpleXmlSerializable, IAssemblyReferenceWhitelistItem {
        public string Name { get; private set; }
        public bool StrictNameCheck { get; private set; }

        private AssemblyReferenceWhitelistFilter() {
        }

        public AssemblyReferenceWhitelistFilter(string name, bool strictNameCheck) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StrictNameCheck = strictNameCheck;
        }

        public override string ToString() {
            return $"{nameof(Name)}: '{Name}', {nameof(StrictNameCheck)}: {StrictNameCheck}";
        }

        #region With.Fody

        public AssemblyReferenceWhitelistFilter WithName(string value) => null;
        public AssemblyReferenceWhitelistFilter WithIsStrictNameCheck(bool value) => null;

        #endregion

        #region ISimpleXmlSerializable

        void ISimpleXmlSerializable.Serialize() {
            base.Serialize();

            Serializer.ProcessStartElement(SimpleXmlSerializer.GetXmlRootName(GetType()));
            {
                Serializer.ProcessAttributeString(nameof(Name), s => Name = s, () => Name);
                Serializer.ProcessAttributeString(nameof(StrictNameCheck), s => StrictNameCheck = Convert.ToBoolean(s), () => Convert.ToString(StrictNameCheck));
            }
            Serializer.ProcessAdvanceOnRead();
            Serializer.ProcessEndElement();
        }

        #endregion
    }
}