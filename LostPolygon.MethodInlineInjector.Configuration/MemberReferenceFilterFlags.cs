using System;

namespace LostPolygon.MethodInlineInjector {
    [Flags]
    public enum MemberReferenceFilterFlags {
        SkipTypes = 1 << 0,
        SkipMethods = 1 << 1,
        SkipProperties = 1 << 2,
        IsRegex = 1 << 5,
        MatchAncestors = 1 << 6,
    }
}