using System;
using UnityEngine;

namespace CoreLib
{
    public class TransformProxy : MonoBehaviour
    {
        public float lookAtAngleOffset = 0;
        
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

        private Transform _lookAtTarget;
        public Transform LookAtTarget
        {
            get => _lookAtTarget;
            set => _lookAtTarget = value;
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

            if (_lookAtTarget != null)
            {
                var position = transform.position;
                var position1 = _lookAtTarget.position;
                var angle = Mathf.Atan2(position1.y - position.y, position1.x - position.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle + lookAtAngleOffset);
            }
        }
    }
}