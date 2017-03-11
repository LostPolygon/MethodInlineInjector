using System;

namespace TestInjecteeLibrary {
    public class TestInjectee {
        public void Complex() {
            Console.WriteLine("Injectee: Begin");

            float a = Math.Abs(-59f);
            float b = 300f;
            Random random = new Random();
            if (random.NextDouble() > 0.5) {
                Console.WriteLine("Injectee: Random > 0.5!");
                if (random.NextDouble() > 0.5) {
                    Console.WriteLine("Injectee: Random still > 0.5!");
                    return;
                }

                Console.WriteLine("Injectee: First Random was > 0.5!");
            }

            Console.WriteLine("Injectee: a * b = " + (a * b));
            Console.WriteLine("Injectee: End");
        }

        public void SingleStatement() {
            Console.WriteLine("Injectee: SingleStatement");
        }

        public void WithParameters(int a, float b) {
            Console.WriteLine("Injectee: Begin");
            Console.WriteLine("Injectee: a + b = " + (a + b));
            Console.WriteLine("Injectee: End");
        }

        public void WithRefParameter(int a, ref float b) {
            Console.WriteLine("Injectee: Begin");
            b += 3.14f;
            Console.WriteLine("Injectee: a + b = " + (a + b));
            Console.WriteLine("Injectee: End");
        }

        public void WithOutParameter(int a, int b, out float c) {
            Console.WriteLine("Injectee: Begin");
            c = a + b;
            Console.WriteLine("Injectee: c = a + b = " + c);
            Console.WriteLine("Injectee: End");
        }

        public int ReturnValue() {
            return -3;
        }

        public int CallResultReturnValue() {
            return Math.Sign(-3);
        }
    }
}