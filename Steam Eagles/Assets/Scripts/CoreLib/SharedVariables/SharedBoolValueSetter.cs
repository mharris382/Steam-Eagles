using UnityEngine;

namespace CoreLib
{
    /// <summary>
    /// updates the value of the shared bool when component is enabled and disabled
    /// <see cref="SharedBool"/>
    /// </summary>
    public class SharedBoolValueSetter : MonoBehaviour
    {
        public SharedBool variable;
        
        private void OnEnable()
        {
            variable.Value = true;
        }
        
        private void OnDisable()
        {
            variable.Value = false;
        }
    }
}