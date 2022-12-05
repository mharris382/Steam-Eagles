using System;
using UnityEngine;
using UnityEngine.U2D;

namespace Utilities
{
    [RequireComponent(typeof(SpriteShapeController))]
    public class JointsOnSpline : MonoBehaviour
    {
        private SpriteShapeController _shape;
           
        private void Awake()
        {
            _shape = GetComponent<SpriteShapeController>();
        }
    }


    [RequireComponent(typeof(Joint2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class SpawnedJoint : MonoBehaviour
    {
        public Rigidbody2D targetAttachment;

        private Action detachAction;

        
        
        public void AttachTo(Rigidbody2D target)
        {
            var fixedJoint = targetAttachment.gameObject.AddComponent<FixedJoint2D>();

            detachAction = () => {
                Destroy(fixedJoint);
            };
            AttachToBody(fixedJoint.attachedRigidbody);
        }

        public abstract void AttachToBody(Rigidbody2D target);
    }
}