using System;

namespace LostPolygon.MethodInlineInjector.Serialization {
    [Flags]
    public enum SimpleXmlSerializerFlags {
        IsOptional = 1 << 0,
        AtLeastOneElement = 1 << 1,
        ExactlyOneElement = 1 << 2,
    }
}
