using System;
using a;
using AI.Enemies;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AI.Bots
{
    [RequireComponent(typeof(Bot))]
    public class BotAIContext : AIContext<Bot>
    {
        public TargetingConfig targetingConfig;
        public float rotationSpeed = 10;
        
        [Serializable]
        public class TargetingConfig
        {
            
            [SerializeField, MinMaxSlider(0, 20, true)]
            private Vector2 effectiveRange = new Vector2(1, 5);

            public float keepTargetSameUtility = 0.2f;
            public float directionUtility = 1;
            public bool IsDistanceInRange(float distance) => distance >= effectiveRange.x && distance <= effectiveRange.y;
            
            public void OnDrawGizmos(Transform position)
            {
                var pos = position.position;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pos, effectiveRange.x);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pos, effectiveRange.y);
            }
        }

        public void RotateTowardsTarget()
        {
            if (HasTarget == false) return;
            Self.RotateTowards(Target.transform.position, rotationSpeed);    
        }

        

        private void OnDrawGizmos()
        {
            targetingConfig.OnDrawGizmos(transform);
        }
    }
}