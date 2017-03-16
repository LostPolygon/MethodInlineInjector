using System;
using System.Text.RegularExpressions;


namespace TestInjectedLibrary {
    public class TestInjectedMethods {
        public static void SingleStatement() {
            Console.WriteLine("Injected: This is injected code!");
        }

        public static void Complex() {
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

        public static void SimpleReturn() {
            Console.WriteLine("Injected: Begin");

            if (Environment.UserInteractive) {
                Console.WriteLine("Injected: Environment.UserInteractive is true, returning");
                return;
            }

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

        public static void Switch() {
            Console.WriteLine("Injected: Start");

            switch (new Random().Next(0, 5)) {
                case 0: Console.WriteLine("Injected: 0"); break;
                case 1: Console.WriteLine("Injected: 1"); break;
                case 2: Console.WriteLine("Injected: 2"); break;
                case 3: Console.WriteLine("Injected: 3"); break;
            }

            Console.WriteLine("Injected: End");
        }
    }
}