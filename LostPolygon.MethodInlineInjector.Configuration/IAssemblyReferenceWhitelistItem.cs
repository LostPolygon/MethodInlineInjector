using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [KnownInheritors(typeof(AssemblyReferenceWhitelistFilter), typeof(AssemblyReferenceWhitelistFilterInclude))]
    public interface IAssemblyReferenceWhitelistItem {
    }
}