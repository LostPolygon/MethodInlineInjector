using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public interface ISimpleXmlSerializable : IXmlSerializable  {
        void Serialize();
        void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable);
    }
}