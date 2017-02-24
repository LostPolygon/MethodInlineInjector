using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInjector {
    public interface ISimpleXmlSerializable : IXmlSerializable  {
        void Serialize();
        void SerializeWithInheritedMode(SimpleXmlSerializable simpleXmlSerializable);
    }
}