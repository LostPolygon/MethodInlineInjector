using System;
using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Assembly")]
    public class AssemblyReferenceWhitelistFilter : IAssemblyReferenceWhitelistItem {
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

        [SerializationMethod]
        public static AssemblyReferenceWhitelistFilter Serialize(AssemblyReferenceWhitelistFilter instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new AssemblyReferenceWhitelistFilter();

            serializer.ProcessStartElement(serializer.GetXmlRootName(typeof(AssemblyReferenceWhitelistFilter)));
            {
                serializer.ProcessAttributeString(nameof(Name), s => instance.Name = s, () => instance.Name);
                serializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional, 
                    () => {
                    serializer.ProcessAttributeString(
                        nameof(StrictNameCheck),
                        s => instance.StrictNameCheck = Convert.ToBoolean(s),
                        () => Convert.ToString(instance.StrictNameCheck)
                    );
                });

            }
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}