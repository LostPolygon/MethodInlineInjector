using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class AllowedAssemblyReferenceInclude : InjectionConfigurationFileInclude, IAllowedAssemblyReference {
        private AllowedAssemblyReferenceInclude() {
        }

        public AllowedAssemblyReferenceInclude(string path) : base(path) {
        }

        [SerializationMethod]
        public static AllowedAssemblyReferenceInclude Serialize(AllowedAssemblyReferenceInclude instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new AllowedAssemblyReferenceInclude();
            InjectionConfigurationFileInclude.Serialize(instance, serializer);
            return instance;
        }
    }
}