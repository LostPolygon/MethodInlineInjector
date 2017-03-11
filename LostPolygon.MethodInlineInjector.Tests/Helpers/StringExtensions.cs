using System;

namespace LostPolygon.MethodInlineInjector.Tests {
    internal static class StringExtensions {
        public static string RemoveSubstringFromEnd(this string input, string substring) {
            if (input.EndsWith(substring)) {
                input = input.Substring(0, input.LastIndexOf(substring, StringComparison.Ordinal));
            }

            return input;
        }

        public static string RemoveAttribute(this string input) {
            return input.RemoveSubstringFromEnd("Attribute");
        }
    }
}
