using System;

namespace LostPolygon.Common.SimpleXmlSerialization {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class KnownInheritorsAttribute : Attribute  {
        public Type[] InheritorTypes { get; }

        public KnownInheritorsAttribute(params Type[] inheritorTypes) {
            InheritorTypes = inheritorTypes;
        }
    }
}