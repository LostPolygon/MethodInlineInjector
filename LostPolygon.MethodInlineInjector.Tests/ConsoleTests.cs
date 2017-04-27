using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net.Config;
using LostPolygon.MethodInlineInjector.Cli;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class ConsoleTests : TestEnvironmentTestsBase {
        [Test]
        public void CommandLineParseTest() {
            AssertOutputContains(ExecuteCommandSimple("lol.xml"), "at least one of --file or --stdin options must be specified", 1);
            AssertOutputContains(ExecuteCommandSimple("-f"), @"Option 'f, file' has no value", 1);
            AssertOutputContains(ExecuteCommandSimple("-f NonExistant.xml"), "Injection configuration file doesn't exists", 1);
            AssertOutputContains(ExecuteCommandSimple("-s -f lol.xml"), "only one of --file or --stdin options is allowed at the same time", 1);
        }

        [Test]
        public void SchemaValidateTest() {
            void Validate(string configurationXml) {
                try {
                    typeof(ConsoleInjector)
                        .GetMethod(
                            "ValidateConfiguration",
                            BindingFlags.NonPublic | BindingFlags.Static)
                        .Invoke(null, new object[] { configurationXml });
                } catch (TargetInvocationException e) {
                    Console.WriteLine(e.InnerException?.InnerException?.Message + "\r\n\r\n");
                    throw e.InnerException;
                }
            }

            AssertOutputContains(ExecuteCommandSimple("-s", "some fake data"), "Data at the root level is invalid", 1);

            const string test1 =
                @"
<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
            <MemberReferenceBlacklist>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </MemberReferenceBlacklist>
            <AssemblyReferenceWhitelist>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <MemberReferenceBlacklist>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </MemberReferenceBlacklist>
          <AssemblyReferenceWhitelist>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
    </InjecteeAssemblies>
    <InjectedMethods>
        <InjectedMethod AssemblyPath=""TestInjectedLibrary.dll"" MethodFullName=""TestInjectedLibrary.TestInjectedMethods.Complex"" InjectionPosition=""InjecteeMethodStart"" />
    </InjectedMethods>
</Configuration>";

            const string test2 =
                @"
<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
            <MemberReferenceBlacklist>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </MemberReferenceBlacklist>
            <AssemblyReferenceWhitelist>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <MemberReferenceBlacklist>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </MemberReferenceBlacklist>
          <AssemblyReferenceWhitelist>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
    </InjecteeAssemblies>
    <InjectedMethods>
        <InjectedMethod Assembly1Path=""TestInjectedLibrary.dll"" MethodFullName=""TestInjectedLibrary.TestInjectedMethods.Complex"" InjectionPosition=""InjecteeMethodStart"" />
    </InjectedMethods>
</Configuration>";

            const string test3 =
                @"
<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
            <MemberReferenceBlacklist>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </MemberReferenceBlacklist>
            <AssemblyReferenceWhitelist>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <MemberReferenceBlacklist>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </MemberReferenceBlacklist>
          <AssemblyReferenceWhitelist>
            <Include Path1=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
    </InjecteeAssemblies>
    <InjectedMethods>
        <InjectedMethod AssemblyPath=""TestInjectedLibrary.dll"" MethodFullName=""TestInjectedLibrary.TestInjectedMethods.Complex"" InjectionPosition=""InjecteeMethodStart"" />
    </InjectedMethods>
</Configuration>";

            const string test4 =
                @"
<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
            <MemberReferenceBlacklist>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </MemberReferenceBlacklist>
            <AssemblyReferenceWhitelist>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <MemberReferenceBlacklist>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </MemberReferenceBlacklist>
          <AssemblyReferenceWhitelist>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AssemblyReferenceWhitelist>
        </InjecteeAssembly>
    </InjecteeAssemblies>
</Configuration>";

            Validate(test1);
            Assert.Catch<MethodInlineInjectorException>(() => Validate(test2));
            Assert.Catch<MethodInlineInjectorException>(() => Validate(test3));
            Assert.Catch<MethodInlineInjectorException>(() => Validate(test4));
        }

        [OneTimeSetUp]
        public static void Setup() {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            XmlConfigurator.Configure();
        }

        private static void AssertOutputContains((string output, int exitCode) result, string needle, int expectedExitCode = 0) {
            Assert.AreEqual(result.exitCode, expectedExitCode, 0, "Exit code invalid");
            Assert.True(result.output.Contains(needle), result.output);
        }

        private static (string output, int exitCode) ExecuteCommandSimple(string arguments, string standardInput = null) {
            return ExecuteCommand(typeof(ConsoleInjector).Namespace + ".exe", arguments, standardInput);
        }

        private static (string output, int exitCode) ExecuteCommand(string fileName, string arguments, string standardInput = null, int timeout = 5000) {
            Process program = new Process {
                StartInfo = {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = standardInput != null,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.Unicode
                }
            };
            program.Start();
            if (standardInput != null) {
                program.StandardInput.WriteLine(standardInput);
                program.StandardInput.Close();
            }

            string result = program.StandardOutput.ReadToEnd();

            bool exited = program.WaitForExit(timeout);
            if (!exited)
                throw new TimeoutException($"'{fileName} {arguments}' did not finish in time");

            return (result.Trim(), program.ExitCode);
        }
    }
}
