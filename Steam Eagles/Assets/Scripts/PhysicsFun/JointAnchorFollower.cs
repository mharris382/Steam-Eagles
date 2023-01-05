using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun
{
    [ExecuteAlways]
    public class JointAnchorFollower : MonoBehaviour
    {
        [Required]
        public Joint2D targetJoint;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _currentPosition;
        private Vector2 _targetPosition;
        private Vector2 _velocity;
        
        private void Awake()
        {
            _rigidbody2D = targetJoint.connectedBody;
        }

        
    
        private void Update()
        {
            if (targetJoint != null)
            {
                if (targetJoint is AnchoredJoint2D anchoredJoint)
                {
                    if(anchoredJoint.connectedBody != null)
                    {
                        transform.position = anchoredJoint.connectedBody.transform.TransformPoint(anchoredJoint.connectedAnchor);
                    }
                    else
                    {
                        
                    }
                }
                _currentPosition = _targetPosition = transform.position;
            }
            else
            {
                _targetPosition = _rigidbody2D.position;
                _currentPosition =transform.position = Vector2.SmoothDamp(_currentPosition, _targetPosition, ref _velocity, 0.1f);
            }
        }
    }
}