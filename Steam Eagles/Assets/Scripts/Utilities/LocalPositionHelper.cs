using System;
using UnityEngine;

namespace Utilities
{
    public class LocalPositionHelper : MonoBehaviour
    {
        [SerializeField] Transform target;
        public Transform Target { get => target;
            set => target = value;
        }
        

        private void Awake()
        {
            target = target == null ? transform : target;
        }

        public void ResetLocalPosition()
        {
            Target.localPosition = Vector3.zero;
        }
        
        public void ResetLocalRotation()
        {
            Target.localRotation = Quaternion.identity;
        }

        public void SetLocalRotation2D(float angle)
        {
            Target.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}