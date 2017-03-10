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
            Console.WriteLine("OnEnable Start");
        }
    }
}