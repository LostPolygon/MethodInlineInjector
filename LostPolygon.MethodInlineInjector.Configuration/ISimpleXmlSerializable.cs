using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public interface ISimpleXmlSerializable : IXmlSerializable  {
        void Serialize();
        void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable);
    }
}