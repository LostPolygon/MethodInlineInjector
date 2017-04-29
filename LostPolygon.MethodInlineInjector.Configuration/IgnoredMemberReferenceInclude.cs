using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Include")]
    public class IgnoredMemberReferenceInclude : InjectionConfigurationFileInclude, IIgnoredMemberReference {
        private IgnoredMemberReferenceInclude() {
        }

        public IgnoredMemberReferenceInclude(string path) : base(path) {
        }

        [SerializationMethod]
        public static IgnoredMemberReferenceInclude Serialize(IgnoredMemberReferenceInclude instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new IgnoredMemberReferenceInclude();
            InjectionConfigurationFileInclude.Serialize(instance, serializer);
            return instance;
        }
    }
}