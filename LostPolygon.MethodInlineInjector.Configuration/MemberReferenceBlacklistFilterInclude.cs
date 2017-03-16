using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class MemberReferenceBlacklistFilterInclude : InjectionConfigurationFileInclude, IMemberReferenceBlacklistItem {
        private MemberReferenceBlacklistFilterInclude() {
        }

        public MemberReferenceBlacklistFilterInclude(string path) : base(path) {
        }
    }
}