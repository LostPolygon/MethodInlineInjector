using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class AssemblyReferenceWhitelistFilterInclude : InjectionConfigurationFileInclude, IAssemblyReferenceWhitelistItem {
        private AssemblyReferenceWhitelistFilterInclude() {
        }

        public AssemblyReferenceWhitelistFilterInclude(string path) : base(path) {
        }
    }
}