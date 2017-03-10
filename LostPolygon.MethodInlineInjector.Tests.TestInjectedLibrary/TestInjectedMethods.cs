using System;
using System.Text.RegularExpressions;


namespace TestInjectedLibrary {
    public class TestInjectedMethods {
        private static int _intField = 5;

        public static void SingleStatement() {
            Console.WriteLine("Injected: This is injected code!");
        }

        public static void ComplexMethod() {
            Console.WriteLine("Injected: Begin");

            Regex regex = new Regex("foo");
            if (regex.IsMatch("test")) {
                Console.WriteLine("Injected: Somehow it matched");
                return;
            }

            int a = Math.Abs(-5);
            double b = 30;
            ushort c = 444;
            Console.WriteLine("Injected: a * b * c = " + (a * b * c));
            Console.WriteLine("Injected: End");
        }

        public static void DeepReturn() {
            Console.WriteLine("Injected: Begin");
            Random random = new Random();
            if (random.NextDouble() > 0.5) {
                Console.WriteLine("Injected: Bad luck :(");
                if (random.NextDouble() > 0.5) {
                    Console.WriteLine("Injected: Super bad luck :(");
                    return;
                }
            }

            Console.WriteLine("Injected: End");
        }

        public static void TryCatch() {
            Console.WriteLine("Injected: Start");

            try {
                Console.WriteLine("Injected: Try");
            } catch (Exception e) {
                Console.WriteLine("Injected: Catch " + e);
                throw;
            } finally {
                Console.WriteLine("Injected: Finally");
            }
            
            Console.WriteLine("Injected: End");
        }

        public static void FieldDependent() {
            Console.WriteLine("Injected: _intField = " + _intField);
        }

        public static void TypeDependent() {
            SomeOtherClass.AddInts(3, 8);
        }

        public void NonStatic() {
            Console.WriteLine("test");
        }
    }
}