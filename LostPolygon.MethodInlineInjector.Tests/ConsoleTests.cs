using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            void Validate(string configurationXml, string assertExceptionNeedle = null) {
                try {
                    typeof(ConsoleInjector)
                        .GetMethod(
                            "ValidateConfiguration",
                            BindingFlags.NonPublic | BindingFlags.Static)
                        .Invoke(null, new object[] { configurationXml });
                } catch (TargetInvocationException e) {
                    string exceptionText = e.ToString();
                    Console.WriteLine(e.InnerException?.InnerException?.Message + "\r\n\r\n");
                    if (assertExceptionNeedle != null) {
                        Assert.True(exceptionText.Contains(assertExceptionNeedle));
                    }

                    Assert.IsAssignableFrom(typeof(MethodInlineInjectorException), e.InnerException);
                }
            }

            AssertOutputContains(ExecuteCommandSimple("-s", "some fake data"), "Data at the root level is invalid", 1);

            const string test1 =
                @"
<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
            <IgnoredMemberReferences>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </IgnoredMemberReferences>
            <AllowedAssemblyReferences>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AllowedAssemblyReferences>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <IgnoredMemberReferences>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </IgnoredMemberReferences>
          <AllowedAssemblyReferences>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AllowedAssemblyReferences>
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
            <IgnoredMemberReferences>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </IgnoredMemberReferences>
            <AllowedAssemblyReferences>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AllowedAssemblyReferences>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <IgnoredMemberReferences>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </IgnoredMemberReferences>
          <AllowedAssemblyReferences>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AllowedAssemblyReferences>
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
            <IgnoredMemberReferences>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </IgnoredMemberReferences>
            <AllowedAssemblyReferences>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AllowedAssemblyReferences>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <IgnoredMemberReferences>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </IgnoredMemberReferences>
          <AllowedAssemblyReferences>
            <Include Path1=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AllowedAssemblyReferences>
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
            <IgnoredMemberReferences>
                <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
            </IgnoredMemberReferences>
            <AllowedAssemblyReferences>
                <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
            </AllowedAssemblyReferences>
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""Mono.Cecil_Injectee.dll"">
          <IgnoredMemberReferences>
            <Filter Filter=""Mono.Cecil.PE"" SkipTypes=""False"" SkipProperties=""True"" IsRegex=""False"" MatchAncestors=""False"" />
          </IgnoredMemberReferences>
          <AllowedAssemblyReferences>
            <Include Path=""TestData/Common/RuntimeAssembliesWhitelist.xml"" />
          </AllowedAssemblyReferences>
        </InjecteeAssembly>
    </InjecteeAssemblies>
</Configuration>";

            Validate(test1);
            Validate(test2, @"The 'Assembly1Path' attribute is not declared.");
            Validate(test3, @"The 'Path1' attribute is not declared.");
            Validate(test4, @"The element 'Configuration' has incomplete content. List of possible elements expected: 'InjectedMethods'");
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

        private static (string output, int exitCode) ExecuteCommand(
            string fileName,
            string arguments,
            string standardInput = null,
            Encoding standardInputEncoding = null,
            int timeout = 5000) {
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
                StreamWriter streamWriter = new StreamWriter(program.StandardInput.BaseStream, standardInputEncoding ?? new UTF8Encoding(false));
                streamWriter.Write(standardInput);
                streamWriter.Close();
            }

            string result = program.StandardOutput.ReadToEnd().Trim();
            bool exited = program.WaitForExit(timeout);
            if (!exited) {
                try {
                    program.Kill();
                } catch {
                    // Ignore
                }

                throw new TimeoutException($"'{fileName} {arguments}' did not finish in time, output: \r\n{result}");
            }

            return (result, program.ExitCode);
        }
    }
}
