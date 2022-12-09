using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class DelayedInvoker : MonoBehaviour
    {
        public float delayTime = 1;
        public UnityEvent onDelayComplete;
        
        public void Invoke()
        {
            Invoke(nameof(InvokeEvent), delayTime);
        }
        
        private void InvokeEvent()
        {
            onDelayComplete?.Invoke();
        }
    }
}