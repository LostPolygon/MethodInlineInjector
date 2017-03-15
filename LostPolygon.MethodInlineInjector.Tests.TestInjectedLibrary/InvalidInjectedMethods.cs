using System;

namespace TestInjectedLibrary {
    public class InvalidInjectedMethods {
        private static int _intField = 5;

        public static void WithParameters(int a, float b) {
            Console.WriteLine("Injected: a + b = " + (a + b));
        }

        public static void WithGenericParameters<T1, T2>() {
            Console.WriteLine($"Injected: T1 = {typeof(T1)}, T2 = {typeof(T2)}");
        }

        public static int WithReturnValue() {
            return 5;
        }

        public static void FieldDependent() {
            Console.WriteLine("Injected: _intField = " + _intField);
        }

        public static void FieldDependentValid() {
            Console.WriteLine("Injected: String.Empty = " + String.Empty);
        }

        public static void TypeDependent() {
            SomeOtherClass.AddInts(3, 8);
        }

        public void NonStatic() {
            Console.WriteLine("test");
        }
    }
}