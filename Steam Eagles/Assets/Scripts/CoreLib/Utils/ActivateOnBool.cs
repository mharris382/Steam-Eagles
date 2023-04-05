using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib
{
    public class ActivateOnBool : MonoBehaviour
    {
        [Required] public SharedBool sharedVariable;
        public bool invert;
        public GameObject targetGameObject;

        
        private void Awake()
        {
            targetGameObject = targetGameObject ? targetGameObject : gameObject;
            sharedVariable.onValueChanged.AddListener(Call);
            Call(sharedVariable.Value);
        }

        private void OnDestroy()
        {
            sharedVariable.onValueChanged.RemoveListener(Call);
        }

        private void Call(bool v)
        {
            if (invert) v = !v;
            targetGameObject.SetActive(v);
        }
    }
}