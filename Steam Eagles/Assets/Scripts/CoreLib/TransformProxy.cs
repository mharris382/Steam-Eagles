using System;
using UnityEngine;

namespace CoreLib
{
    public class TransformProxy : MonoBehaviour
    {
        private Transform _target;
        public Transform Target
        {
            get => _target == null ? transform : _target;
            set
            {
                _target = value;
                enabled = _target != null;
            }
        }

        private void Update()
        {
            transform.position = Target.position;
        }
    }
}