using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffLib;
using NUnit.Framework;

// Taken from ICSharpCode.Decompiler.Tests.Helpers
namespace LostPolygon.MethodInlineInjector.Tests {
    public class CodeAssert {
        public static void AreEqual(string input1, string input2) {
            StringWriter diff = new StringWriter();
            if (!Compare(input1, input2, diff)) {
                Assert.Fail(diff.ToString());
            }
        }

        public static bool Compare(string input1, string input2, StringWriter diff) {
            List<string> input1List = NormalizeAndSplitCode(input1).ToList();
            List<string> input2List = NormalizeAndSplitCode(input2).ToList();

            IEnumerable<DiffSection> diffSections = Diff.CalculateSections(input1List, input2List, new CodeLineEqualityComparer());
            IEnumerable<DiffElement<string>> diffElements = Diff.AlignElements(
                input1List,
                input2List,
                diffSections,
                new StringSimilarityDiffElementAligner());

            bool result = true;
            int line1 = 0, line2 = 0;

            foreach (DiffElement<string> change in diffElements) {
                bool ignoreChange;
                switch (change.Operation) {
                    case DiffOperation.Match:
                        diff.Write("{0,4} {1,4} ", ++line1, ++line2);
                        diff.Write("  ");
                        diff.WriteLine(change.ElementFromCollection1);
                        break;
                    case DiffOperation.Insert:
                        diff.Write("     {1,4} ", line1, ++line2);
                        result &= ignoreChange = ShouldIgnoreChange(change.ElementFromCollection2.Value);
                        diff.Write(ignoreChange ? "    " : " +  ");
                        diff.WriteLine(change.ElementFromCollection2);
                        break;
                    case DiffOperation.Delete:
                        diff.Write("{0,4}      ", ++line1, line2);
                        result &= ignoreChange = ShouldIgnoreChange(change.ElementFromCollection1.Value);
                        diff.Write(ignoreChange ? "    " : " -  ");
                        diff.WriteLine(change.ElementFromCollection1);
                        break;
                    case DiffOperation.Replace:
                        diff.Write("{0,4}      ", ++line1, line2);
                        result = false;
                        diff.Write("(-) ");
                        diff.WriteLine(change.ElementFromCollection1);
                        diff.Write("     {1,4} ", line1, ++line2);
                        diff.Write("(+) ");
                        diff.WriteLine(change.ElementFromCollection2);
                        break;
                    case DiffOperation.Modify:
                        diff.Write("{0,4}      ", ++line1, line2);
                        result = false;
                        diff.Write("(-) ");
                        diff.WriteLine(change.ElementFromCollection1);
                        diff.Write("     {1,4} ", line1, ++line2);
                        diff.Write("(*) ");
                        diff.WriteLine(change.ElementFromCollection2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        private static string NormalizeLine(string line) {
            line = line.Trim();
            int index = line.IndexOf("//", StringComparison.Ordinal);
            if (index >= 0) {
                return line.Substring(0, index);
            }
            if (line.StartsWith("#")) {
                return string.Empty;
            }
            return line;
        }

        private static bool ShouldIgnoreChange(string line) {
            // for the result, we should ignore blank lines and added comments
            return NormalizeLine(line) == string.Empty;
        }

        private static IEnumerable<string> NormalizeAndSplitCode(string input) {
            return input.Split(new[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private class CodeLineEqualityComparer : IEqualityComparer<string> {
            private readonly IEqualityComparer<string> _baseComparer = EqualityComparer<string>.Default;

            public bool Equals(string x, string y) {
                return _baseComparer.Equals(
                    NormalizeLine(x),
                    NormalizeLine(y)
                );
            }

            public int GetHashCode(string obj) {
                return _baseComparer.GetHashCode(NormalizeLine(obj));
            }
        }
    }
}