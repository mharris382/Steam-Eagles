using UnityEngine;

namespace CoreLib
{
    public class ToggleSharedBoolOnKeystroke : MonoBehaviour
    {
        public SharedBool variable;
        public KeyCode key;
        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                variable.Value = !variable.Value;
            }
        }
    }
}