namespace LostPolygon.MethodInlineInjector.Serialization {
    public interface ISimpleXmlSerializable {
        void Serialize();

        SimpleXmlSerializerBase Serializer { get; set; }
    }
}