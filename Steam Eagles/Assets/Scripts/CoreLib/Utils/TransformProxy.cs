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

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if(_rigidbody==null)
                transform.position = Target.position;
            else
                _rigidbody.MovePosition(Target.position);
        }
    }
}