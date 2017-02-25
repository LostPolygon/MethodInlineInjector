using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInlineInjector {
    public interface ISimpleXmlSerializable : IXmlSerializable  {
        void Serialize();
        void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable);
    }
}