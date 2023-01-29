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

        [SerializeField] private bool overrideGroundMask = false;
        
        [SerializeField] private LayerMask groundMask = 1;
       public float groundCheckRadius = 0.2f;
        
        public float maxSlopeAngle = 75;

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
    }
}