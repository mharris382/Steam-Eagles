using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AI.Enemies
{
    [Serializable]
    public class GroundCheckConfig
    {
        [Required]
        public Transform groundCheck;
        public float groundCheckDistance = 1;
        public LayerMask groundLayer;

        public bool debug;

        public bool CheckGrounded()
        {
            var hit = Physics2D.Raycast(groundCheck.position, -groundCheck.up, groundCheckDistance, groundLayer);
            if (debug)
            {
                Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);
            }
            return hit;
        }

        public void DrawGizmos(Transform _)
        {
            Gizmos.color = Color.red;
            if(groundCheck == null) return;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + -groundCheck.up * groundCheckDistance);
        }
    }
    [Serializable]
    public class LedgeCheckConfig : GroundCheckConfig
    {
        [MinValue("groundCheckDistance")]
        public float maxDroppableLedgeHeight = 2;

        
        public bool CheckForDroppableLedge()
        {
            if (CheckGrounded()) return false;
            var hit = Physics2D.Raycast(groundCheck.position, -groundCheck.up, maxDroppableLedgeHeight, groundLayer);
            if (debug && hit) Debug.DrawLine(groundCheck.position, hit.point, Color.cyan);
            return hit;
        }
    }

}