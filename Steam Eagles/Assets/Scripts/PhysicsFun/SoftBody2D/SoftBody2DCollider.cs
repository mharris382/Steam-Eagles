using System;
using UnityEngine;

namespace PhysicsFun
{
    [RequireComponent(typeof(SpringJoint2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SoftBody2DCollider : MonoBehaviour
    {
        
        private CircleCollider2D _circleCollider2D;
        private Rigidbody2D _rigidbody;
        
        public CircleCollider2D circleCollider2D => _circleCollider2D ? _circleCollider2D : _circleCollider2D = GetComponent<CircleCollider2D>();
        public Rigidbody2D rigidbody => _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody2D>();


        private void Awake()
        {
            var springs = GetComponents<SpringJoint2D>();
        }

        private SpringJoint2D[] _springs;
        public SpringJoint2D[] Springs => _springs==null || _springs.Length < 3 ? _springs : _springs = GetComponents<SpringJoint2D>();
        public SpringJoint2D GetSpringToMiddle()
        {
            if(_springs.Length < 3)
                _springs = GetComponents<SpringJoint2D>();
            return Springs[1];
        }
        public SpringJoint2D GetSpringToNextBody()
        {
            if(_springs.Length < 3)
                _springs = GetComponents<SpringJoint2D>();
            try
            {
                return Springs[2];
            }
            catch (IndexOutOfRangeException e)
            {
                _springs = GetComponents<SpringJoint2D>();
                Debug.Assert(_springs.Length >= 3, $"Softbody should have at least 3 springs.  Found {_springs.Length} springs.", this);
                return _springs[2];
            }
        }
        public SpringJoint2D GetSpringToPrevBody()
        {
            if(_springs.Length < 3)
                _springs = GetComponents<SpringJoint2D>();
            return Springs[0];
        }
    }
}