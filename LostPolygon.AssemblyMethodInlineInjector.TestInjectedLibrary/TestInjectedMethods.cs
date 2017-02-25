using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TestInjectedLibrary {
    public class TestInjectedMethods {
        private static int _intField = 5;

        public static void InjectedMethod1() {
            Debug.Log("Injected code from InjectedMethod1!");
        }

        public static void InjectedMethod2() {
            Debug.Log("Injected code start! ");

            Regex regex = new Regex("lol");
            if (regex.IsMatch("test")) {
                Debug.Log("lool");
            }

            int a = Mathf.Abs(-5);
            double b = 30;
            ushort c = 444;
            Debug.Log("Injected code end! Some data: " + (a * b * c));
        }

        public static void InjectedMethod_FieldDependent() {
            Debug.Log("Injected code start! ");

            Debug.Log("Injected code end! Some data: " + (_intField));
        }

        public static void InjectedMethod_TypeDependent() {
            SomeOtherClass.AddInts(3, 8);
            //Debug.Log("Injected code start! ");

            //Debug.Log("Injected code end! Some data: " + SomeOtherClass.AddInts(3, 8));
        }

        public static void InjectedMethod_Return() {
            if (Random.value > 0.5f) {
                Debug.Log("Bad luck :(");
                if (Random.value > 0.5f) {
                    Debug.Log("Super bad luck :(");
                    return;
                }
            }

            Debug.Log("Lucky!");
        }

        public static void InjectedMethod_TryCatch() {
            Debug.Log("Start InjectedMethod_TryCatch");
            try {
                Debug.Log("Try InjectedMethod_TryCatch");
            } catch (Exception e) {
                Debug.Log("Catch InjectedMethod_TryCatch: " + e);
                throw;
            } finally {
                Debug.Log("Finally InjectedMethod_TryCatch");
            }
            
            Debug.Log("End InjectedMethod_TryCatch");
        }
    }
}