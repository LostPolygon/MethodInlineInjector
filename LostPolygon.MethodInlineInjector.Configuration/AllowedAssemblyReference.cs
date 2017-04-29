using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Assembly")]
    public class AllowedAssemblyReference : IAllowedAssemblyReference {
        public string Name { get; private set; }
        public bool StrictNameCheck { get; private set; }
        public string Path { get; private set; }

        private AllowedAssemblyReference() {
        }

        public AllowedAssemblyReference(string name, bool strictNameCheck = false, string path = null) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StrictNameCheck = strictNameCheck;
            Path = path;
        }

        public override string ToString() {
            string str = $"{nameof(Name)}: '{Name}', {nameof(StrictNameCheck)}: {StrictNameCheck}";
            if (!String.IsNullOrEmpty(Path)) {
                str = $"{str}, {nameof(Path)}: {Path}";
            }
            return str;
        }

        #region With.Fody

        public AllowedAssemblyReference WithName(string value) => null;
        public AllowedAssemblyReference WithIsStrictNameCheck(bool value) => null;
        public AllowedAssemblyReference WithPath(string value) => null;

        #endregion

        #region ISimpleXmlSerializable

        [SerializationMethod]
        public static AllowedAssemblyReference Serialize(AllowedAssemblyReference instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new AllowedAssemblyReference();

            serializer.ProcessStartElement(serializer.GetXmlRootName(typeof(AllowedAssemblyReference)));
            {
                serializer.ProcessAttributeString(nameof(Name), s => instance.Name = s, () => instance.Name);
                serializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional,
                    () => {
                        serializer.ProcessAttributeString(nameof(Path), s => instance.Path = s, () => instance.Path);
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