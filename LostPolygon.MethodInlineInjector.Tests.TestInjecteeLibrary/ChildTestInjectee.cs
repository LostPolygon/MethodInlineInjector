using System;

namespace TestInjecteeLibrary {
    public class ChildTestInjectee : TestInjectee {
        public override void VirtualSingleStatement() {
            Console.WriteLine("ChildInjectee: VirtualSingleStatement");
        }

        public override int VirtualSingleStatementProperty {
            get {
                Console.WriteLine("ChildInjectee: VirtualSingleStatementProperty Get");
                return 0;
            }
            set {
                Console.WriteLine("ChildInjectee: VirtualSingleStatementProperty Set");
            }
        }
    }
}