using System;

namespace TestInjecteeLibrary {
    public class ChildTestInjectee : TestInjectee {
        public override void SingleStatement() {
            Console.WriteLine("ChildInjectee: SingleStatement");
        }
    }
}