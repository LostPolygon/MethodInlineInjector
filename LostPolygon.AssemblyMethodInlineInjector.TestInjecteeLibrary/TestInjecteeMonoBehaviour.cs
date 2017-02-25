using UnityEngine;

namespace TestLibrary {
    public class TestInjecteeMonoBehaviour : MonoBehaviour {
        private void Update() {
            Debug.Log("Update Start");

            float a = Mathf.Abs(-59f);
            float b = 300f;
            if (Random.value > 0.5f) {
                Debug.Log("Random > 0.5!");
                if (Random.value > 0.5f) {
                    Debug.Log("Random still > 0.5!");
                    return;
                }

                Debug.Log("First Random was > 0.5!");
            }
            
            Debug.Log("Update End" + (a * b));
        }

        private void OnEnable() {
            Debug.Log("OnEnable Start");
        }
    }
}