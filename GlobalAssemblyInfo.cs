using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Lost Polygon")]
[assembly: AssemblyCopyright("© Lost Polygon")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#elif RELEASE
[assembly: AssemblyConfiguration("Release")]
#else
[assembly: AssemblyConfiguration("Unknown")]
#endif

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(Version.kVersion)]
[assembly: AssemblyFileVersion(Version.kVersion)]
[assembly: AssemblyInformationalVersion(Version.kVersion)]

internal static class Version {
    internal const string kVersion = "0.1.0.0";
}