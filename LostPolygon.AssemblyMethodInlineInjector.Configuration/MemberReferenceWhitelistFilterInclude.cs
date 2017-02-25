using System.Collections.Generic;
using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInlineInjector {
    [XmlRoot("MemberReferenceWhitelist")]
    public class MemberReferenceWhitelistFilterInclude : SimpleXmlSerializable {
        public List<InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter> MemberReferenceWhitelist { get; set; } 
            = new List<InjectionConfiguration.InjecteeAssembly.MemberReferenceWhitelistFilter>();

        public override void Serialize() {
            base.Serialize();

            // Skip root element when reading
            SerializationHelper.ProcessAdvanceOnRead();

            this.ProcessCollection(MemberReferenceWhitelist);
        }
    }
}