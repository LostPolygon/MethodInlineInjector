using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using devtm.Cecil.Extensions;
using Mono.Cecil;
using NUnit.Framework;

namespace LostPolygon.MethodInlineInjector.Tests {
    public static class IntegrationTestsHelper {
        public const string kEndOfILCodeSeparator = "// End of IL code";

        public static string GetInjectedMethodFullName(string injectedMethodName) {
            return $"{GetTestProperty(nameof(IntegrationTestsBase.InjectedClassName))}.{injectedMethodName}";
        }

        public static string GetReferenceOutputFilePath() {
            string path =
                Path.Combine(
                    TestEnvironmentConfig.Instance.ProjectDir,
                    "ReferenceOutput",
                    TestContext.CurrentContext.Test.ClassName.Split('.').Last(),
                    TestContext.CurrentContext.Test.MethodName + ".ilcs"
                );

            return path;
        }

        public static void AssertFirstMethod(ResolvedInjectionConfiguration resolvedInjectionConfiguration) {
            AssertMethod(resolvedInjectionConfiguration.InjecteeAssemblies[0].InjecteeMethodsDefinitions[0]);
        }

        public static void AssertMethod(MethodDefinition methodDefinition) {
            Assert.False(methodDefinition == null);

            (string referenceIL, string referenceCSharp) = GetReferenceOutputFile();
            (string currentIL, string currentCSharp, string _) = CreateCombinedDecompiledILAndCSharpCode(methodDefinition);

            StringWriter diffIL = new StringWriter();
            if (!CodeAssert.Compare(referenceIL, currentIL, diffIL)) {
                StringWriter diffCSharp = new StringWriter();
                CodeAssert.Compare(referenceCSharp, currentCSharp, diffCSharp);

                string message = FormatReferenceOutput(diffIL.ToString().Trim(), diffCSharp.ToString().Trim());

                Assert.Fail(message);
            }

            Console.WriteLine(FormatReferenceOutput(currentIL, currentCSharp));
        }

        private static string FormatReferenceOutput(string il, string cSharp) {
            return
                $"IL:{Environment.NewLine}{Environment.NewLine}{il}{Environment.NewLine}{Environment.NewLine}" +
                $"C#:{Environment.NewLine}{Environment.NewLine}{cSharp}";
        }

        public static string GetFormattedReferenceOutputFile() {
            (string il, string cSharp) = GetReferenceOutputFile();
            return FormatReferenceOutput(il, cSharp);
        }

        public static (string ilCode, string cSharpCode) GetReferenceOutputFile() {
            string path = GetReferenceOutputFilePath();
            Assert.True(File.Exists(path), $"Reference output file '{path}' not found");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string combinedCode = File.ReadAllText(path);
            string[] split = combinedCode.Split(new[] { kEndOfILCodeSeparator }, StringSplitOptions.RemoveEmptyEntries);

            return (NormalizeCode(split[0]), NormalizeCode(split[1]));
        }

        public static void WriteReferenceOutputFile(ResolvedInjectionConfiguration resolvedInjectionConfiguration) {
            WriteReferenceOutputFile(resolvedInjectionConfiguration.InjecteeAssemblies[0].InjecteeMethodsDefinitions[0]);
        }

        public static void WriteReferenceOutputFile(MethodDefinition methodDefinition) {
            string path = GetReferenceOutputFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            (string _, string _, string decompiled) = CreateCombinedDecompiledILAndCSharpCode(methodDefinition);
            File.WriteAllText(path, decompiled);
        }

        public static (string ilCode, string cSharpCode, string combinedCode) CreateCombinedDecompiledILAndCSharpCode(MethodDefinition method) {
            StringBuilder sb = new StringBuilder();

            string ilCode = NormalizeCode(DecompilationHelpers.GetMethodDecompiledILCode(method));
            sb.Append(ilCode);

            sb.AppendLine();
            sb.AppendLine();
            sb.Append(kEndOfILCodeSeparator);
            sb.AppendLine();
            sb.AppendLine();

            string cSharpCode = NormalizeCode(DecompilationHelpers.GetMethodDecompiledCSharpCode(method));
            sb.Append(cSharpCode);

            return (ilCode, cSharpCode, sb.ToString());
        }

        public static InjectionConfiguration GetBasicInjectionConfiguration(params InjectionConfiguration.InjectedMethod[] injectedMethods) {
            InjectionConfiguration configuration = new InjectionConfiguration();
            configuration.InjecteeAssemblies.Add(new InjectionConfiguration.InjecteeAssembly(GetTestProperty<string>(nameof(IntegrationTestsBase.InjecteeLibraryName))));
            configuration.InjectedMethods.AddRange(injectedMethods);

            return configuration;
        }

        public static void ExecuteInjection(ResolvedInjectionConfiguration resolvedInjectionConfiguration) {
            MethodInlineInjector assemblyMethodInjector = new MethodInlineInjector(resolvedInjectionConfiguration);
            assemblyMethodInjector.Inject();
        }

        public static ResolvedInjectionConfiguration GetBasicResolvedInjectionConfiguration(InjectionConfiguration injectionConfiguration, params string[] injecteeMethodNames) {
            InjecteeMethodsOverrideResolvedInjectionConfigurationLoader loader =
                new InjecteeMethodsOverrideResolvedInjectionConfigurationLoader(injectionConfiguration, injecteeMethodNames);
            return loader.Load();
        }

        private static string NormalizeCode(string code) {
            code = code.Trim().Replace("\t", "    ");
            code = Regex.Replace(code, @"\r\n|\n\r|\n|\r", "\r\n");

            // Trim trailing whitespace
            code = Regex.Replace(code, @"[ \t]+(\r?$)", @"$1", RegexOptions.Multiline);

            return code;
        }

        private static object GetTestProperty(string key) {
            return TestContext.CurrentContext.Test.Properties.Get(key);
        }

        private static T GetTestProperty<T>(string key) {
            return (T) TestContext.CurrentContext.Test.Properties.Get(key);
        }

        public class InjecteeMethodsOverrideResolvedInjectionConfigurationLoader : ResolvedInjectionConfigurationLoader {
            private readonly string[] _injecteeMethodNames;

            public InjecteeMethodsOverrideResolvedInjectionConfigurationLoader(InjectionConfiguration injectionConfiguration, params string[] injecteeMethodNames)
                : base(injectionConfiguration) {
                _injecteeMethodNames = injecteeMethodNames;
            }

            protected override List<MethodDefinition> FilterInjecteeMethods(List<MethodDefinition> injecteeMethods) {
                List<MethodDefinition> overrideMethodDefinitions =
                    injecteeMethods
                        .Where(method => _injecteeMethodNames.Contains(method.GetFullName()))
                        .ToList();

                return base.FilterInjecteeMethods(overrideMethodDefinitions);
            }
        }
    }
}