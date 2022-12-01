using System;
using UnityEngine;

namespace Utilities
{
    public  class SpawnedSpringJoint : MonoBehaviour
    {
        public Action detachAction;
        public void AttachTo(Rigidbody2D target, Action detachAction)
        {
            this.detachAction = detachAction;
        }

        public void Detach()
        {
            
        }
    }
}