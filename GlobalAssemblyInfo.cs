using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Lost Polygon")]
[assembly: AssemblyCopyright("© Lost Polygon")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#elif RELEASE
[assembly: AssemblyConfiguration("Release")]
#else
[assembly: AssemblyConfiguration("Unknown")]
#endif

[assembly: ComVisible(false)]

#pragma warning disable 0436

[assembly: AssemblyVersion(Version.kVersion)]
[assembly: AssemblyFileVersion(Version.kVersion)]
[assembly: AssemblyInformationalVersion(Version.kVersion)]

#pragma warning restore 0436

internal static class Version {
    internal const string kVersion = "0.1.0.0";
}