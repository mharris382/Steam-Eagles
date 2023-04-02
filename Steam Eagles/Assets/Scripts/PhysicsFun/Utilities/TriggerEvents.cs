using System;
using UnityEngine;
using UnityEngine.Events;

namespace PhysicsFun
{
    public class TriggerEvents : MonoBehaviour
    {
        public string triggerTag = "Builder";
        
        
        public UnityEvent onTriggerEnter;
        public UnityEvent onTriggerExit;

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (col.attachedRigidbody == null)
                return;
            var rb = col.attachedRigidbody;
            if (rb.CompareTag(triggerTag))
            {
                onTriggerEnter?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.attachedRigidbody == null)
                return;
            var rb = col.attachedRigidbody;
            if (rb.CompareTag(triggerTag))
            {
                onTriggerExit?.Invoke();
            }
        }
    }
}