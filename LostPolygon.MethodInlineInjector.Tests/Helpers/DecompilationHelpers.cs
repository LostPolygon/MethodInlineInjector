using System.IO;
using System.Text;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.ILSpy;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector.Tests {
    public static class DecompilationHelpers {
        public static string GetMethodDecompiledCSharpCode(MethodDefinition method) {
            StringBuilder sb = new StringBuilder();
            PlainTextOutput textOutput = new PlainTextOutput(new StringWriter(sb));
            GetMethodDecompiledCSharpCode(textOutput, method);
            return sb.ToString();
        }

        private static void GetMethodDecompiledCSharpCode(PlainTextOutput textOutput, MethodDefinition method) {
            DecompilerSettings decompilerSettings = new DecompilerSettings {
                UsingDeclarations = false,
                UsingStatement = false,
                ShowXmlDocumentation = false,
            };

            CSharpLanguage language = new CSharpLanguage();
            language.DecompileMethod(method, textOutput, decompilerSettings);
        }

        public static string GetMethodDecompiledILCode(MethodDefinition method) {
            StringBuilder sb = new StringBuilder();
            PlainTextOutput textOutput = new PlainTextOutput(new StringWriter(sb));
            GetMethodDecompiledILCode(textOutput, method);
            return sb.ToString();
        }

        public static void GetMethodDecompiledILCode(ITextOutput textOutput, MethodDefinition method) {
            ReflectionDisassembler reflectionDisassembler = new ReflectionDisassembler(textOutput, false, CancellationToken.None);
            reflectionDisassembler.DisassembleMethod(method);
        }
    }
}