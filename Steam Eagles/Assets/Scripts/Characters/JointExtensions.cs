using System;
using UnityEngine;

namespace Characters
{
    public static class JointExtensions
    {
        public static void ApplyVelocityViaConnectedAnchor(this FixedJoint2D fixedJoint, Vector2 velocity, float deltaTime)
        {
            if(velocity.sqrMagnitude <= 0.1)
                return;
            fixedJoint.autoConfigureConnectedAnchor = false;
            var newConnectedAnchor = fixedJoint.connectedAnchor + velocity * deltaTime;
            fixedJoint.connectedAnchor = newConnectedAnchor;
        }
        
        public static void ApplyVelocityViaAnchor(this FixedJoint2D fixedJoint, Vector2 velocity, float deltaTime)
        {
            if(velocity.sqrMagnitude <= 0.1)
                return;
            var newAnchor = fixedJoint.anchor + velocity * deltaTime;
            fixedJoint.anchor = newAnchor;
        }
        
        
        public static void ApplyVelocityViaConnectedAnchor(this FixedJoint2D fixedJoint, Vector2 velocity, float deltaTime, Func<Vector2, Vector2> postProcessAnchor)
        {
            if(velocity.sqrMagnitude <= 0.1)
                return;
            fixedJoint.autoConfigureConnectedAnchor = false;
            var newConnectedAnchor = fixedJoint.connectedAnchor + velocity * deltaTime;
            fixedJoint.connectedAnchor = postProcessAnchor(newConnectedAnchor);
        }
        
        public static void ApplyVelocityViaAnchor(this FixedJoint2D fixedJoint, Vector2 velocity, float deltaTime, Func<Vector2, Vector2> postProcessAnchor)
        {
            if(velocity.sqrMagnitude <= 0.1)
                return;
            var newAnchor = fixedJoint.anchor + velocity * deltaTime;
            fixedJoint.anchor = postProcessAnchor(newAnchor);
        }
    }
}