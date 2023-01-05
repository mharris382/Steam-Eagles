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
        public float groundedFriction = 0.4f;
        public float gravityScaleFall = 3;
        public float gravityScaleJump = 5;

        [SerializeField] private bool overrideGroundMask = false;
        [ShowIf(nameof(overrideGroundMask))]
        [SerializeField] private LayerMask groundMask = 1;

        
        public float maxSlopeAngle = 75;

        public LayerMask GetGroundLayers()
        {
            return overrideGroundMask ? groundMask : LayerMask.GetMask("Ground", "Solids");
        }

        public PhysicsMaterial2D GetNoFrictionMaterial()
        {
            var mat = new PhysicsMaterial2D()
            {
                friction = 0,
                bounciness = 0
            };
            return mat;
        }

        public PhysicsMaterial2D GetFullFrictionMaterial()
        {
            var mat = new PhysicsMaterial2D()
            {
                friction = groundedFriction,
                bounciness = 0
            };
            return mat;
        }
    }
}