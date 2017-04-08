using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class MemberReferenceBlacklistFilterInclude : InjectionConfigurationFileInclude, IMemberReferenceBlacklistItem {
        private MemberReferenceBlacklistFilterInclude() {
        }

        public MemberReferenceBlacklistFilterInclude(string path) : base(path) {
        }

        [SerializationMethod]
        public static MemberReferenceBlacklistFilterInclude Serialize(MemberReferenceBlacklistFilterInclude instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new MemberReferenceBlacklistFilterInclude();
            InjectionConfigurationFileInclude.Serialize(instance, serializer);
            return instance;
        }
    }
}