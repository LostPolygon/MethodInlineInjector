using System;

namespace LostPolygon.Common.SimpleXmlSerialization {
    [Flags]
    public enum SimpleXmlSerializerFlags {
        IsOptional = 1 << 0,
        AtLeastOneElement = 1 << 1,
        ExactlyOneElement = 1 << 2,
    }
}
