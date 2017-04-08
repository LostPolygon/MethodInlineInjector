using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class AssemblyReferenceWhitelistFilterInclude : InjectionConfigurationFileInclude, IAssemblyReferenceWhitelistItem {
        private AssemblyReferenceWhitelistFilterInclude() {
        }

        public AssemblyReferenceWhitelistFilterInclude(string path) : base(path) {
        }

        [SerializationMethod]
        public static AssemblyReferenceWhitelistFilterInclude Serialize(AssemblyReferenceWhitelistFilterInclude instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new AssemblyReferenceWhitelistFilterInclude();
            InjectionConfigurationFileInclude.Serialize(instance, serializer);
            return instance;
        }
    }
}