using UnityEngine;

namespace TestLibrary {
    public class TestInjecteeMonoBehaviour : MonoBehaviour {
        private void Update() {
            Debug.Log("Update Start");
            mark:
            float a = Mathf.Abs(-59f);
            float b = 300f;
            if (Random.value > 0.5f) {
                Debug.Log("Random > 0.5!");
                goto mark;
            }
            
            Debug.Log("Update End" + (a * b));
        }

        private void OnEnable() {
            Debug.Log("OnEnable Start");
        }
    }
}