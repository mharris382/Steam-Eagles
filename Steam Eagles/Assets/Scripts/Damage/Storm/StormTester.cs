using System;
using UnityEngine;

namespace Damage
{
    public class StormTester : MonoBehaviour
    {
        private StormInstance _handle;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) && _handle == null)
            {
                Debug.Log("Starting storm");
                _handle= StormManager.Instance.StartStorm(1);
            }

            if (Input.GetKeyDown(KeyCode.F2) && _handle != null)
            {
                Debug.Log("Stopping Storm");
                StormManager.Instance.StopStorm(_handle);
                _handle = null;
            }
        }
    }
}