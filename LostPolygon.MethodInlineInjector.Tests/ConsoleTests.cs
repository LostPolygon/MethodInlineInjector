using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
            AssertOutputContains(ExecuteCommandSimple("-s", "some invalid input"), "Data at the root level is invalid", 1);
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
