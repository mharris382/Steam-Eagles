using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "New Character Config", order = 0)]
    public class CharacterConfig : ScriptableObject
    {
        public float moveSpeed = 600;
        public float jumpForce = 15f;
        public float jumpTime = 0.3f;
        
        public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        public float groundedFriction = 0.4f;
        public float walkingFriction = 0.1f;
        public float gravityScaleFall = 3;
        public float gravityScaleJump = 5;

        public float balloonJumpCoyoteTime = 0.2f;
        public float balloonJumpMultiplier = 2;
        public float balloonImpactForce = 20;
        [SerializeField] private bool overrideGroundMask = false;
        
        [SerializeField] private LayerMask groundMask = 1;
       public float groundCheckRadius = 0.2f;
        
        public float maxSlopeAngle = 75;
        
        [SerializeField] private ClimbConfig climbConfig = new ClimbConfig();

        public LayerMask GetGroundLayers()
        {
            return groundMask;
        }

        public PhysicsMaterial2D GetNoFrictionMaterial()
        {
            var mat = new PhysicsMaterial2D()
            {
                friction = 0,
                bounciness = 0
            };
            mat.name = "Frictionless Material";
            return mat;
        }
        public PhysicsMaterial2D GetWalkingFrictionMaterial()
        {
            var mat = new PhysicsMaterial2D()
            {
                friction = walkingFriction,
                bounciness = 0
            };
            
            mat.name = "Walking Material";
            return mat;
        }


        public PhysicsMaterial2D GetFullFrictionMaterial()
        {
            var mat = new PhysicsMaterial2D()
            {
                friction = groundedFriction,
                bounciness = 0
            };
            mat.name = $"Full Friction Material {groundedFriction:F2}";
            return mat;
        }
        
        
        [Serializable]
        private class ClimbConfig
        {
            [SerializeField] protected internal float climbSpeedBase = 5;
            [SerializeField] protected internal float climbUpSpeedMultiplier = 1;
            [SerializeField] protected internal float climbUpSprintSpeedMultiplier = 1.5f;
            [SerializeField] protected internal float climbDownSpeedMultiplier = 1.5f;
            [SerializeField] protected internal float climbDownSprintSpeedMultiplier = 1.5f;
            
            [Tooltip("Force multiplier that is applied when player initiates a jump while climbing")]
            [SerializeField] protected internal float climbJumpMultiplier = 0.8f;
            
        }

        public float GetClimbSpeed(float climbDirection, bool isSprintDown)
        {
            var speed = climbConfig.climbSpeedBase;
            if(climbDirection > 0)
                speed *= isSprintDown ? climbConfig.climbUpSprintSpeedMultiplier : climbConfig.climbUpSpeedMultiplier;
            else if(climbDirection < 0)
                speed *= isSprintDown ? climbConfig.climbDownSprintSpeedMultiplier : climbConfig.climbDownSpeedMultiplier;
            return speed;
        }
        
    }
}