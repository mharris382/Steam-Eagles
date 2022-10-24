using System;
using UnityEngine;
using UnityEngine.Events;

namespace CoreLib
{
    public class KeystrokeEvent : MonoBehaviour
    {
        public KeyCode key;
        public UnityEvent onButtonPressed;
        public UnityEvent onButtonReleased;


        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                onButtonPressed?.Invoke();
            }

            if (Input.GetKeyUp(key))
            {
                onButtonPressed?.Invoke();
            }
        }
    }
}