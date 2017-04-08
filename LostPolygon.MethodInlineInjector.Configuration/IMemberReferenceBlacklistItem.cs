using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [KnownInheritors(typeof(MemberReferenceBlacklistFilter), typeof(MemberReferenceBlacklistFilterInclude))]
    public interface IMemberReferenceBlacklistItem {
    }
}