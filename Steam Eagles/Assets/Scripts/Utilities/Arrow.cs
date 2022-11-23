using System;
using UnityEngine;

namespace Utilities
{
    
    [RequireComponent(typeof(DestructionHandler))]
    public class Arrow : MonoBehaviour
    {
        public LayerMask stickToLayers;
        public LayerMask destroyOnLayers;
        
        public float threshold = 0.5f;

        
        Rigidbody2D _rb;
        private Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
        private DestructionHandler _destruction;
        private DestructionHandler destruction => _destruction ? _destruction : _destruction = GetComponent<DestructionHandler>();
        private SpriteRenderer _sr;
        private SpriteRenderer sr => _sr ? _sr : _sr = GetComponentInChildren<SpriteRenderer>();

        void StickTo(Collider2D collider, Vector3 position, Vector3 normal)
        {
            var joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = collider.attachedRigidbody;
            joint.enableCollision = true;
            joint.breakForce = 1000;
            joint.breakTorque = 1000;
        }

        void DestroyArrow()
        {
            
        }
    }
}