using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    /// <summary>
    /// one of these should be added to each character and the unity event will allow them to communicate
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class DynamicBlockCollector : TriggerArea<DynamicBlock>
    {
        public string targetTag = "DynamicBlock";
        private CircleCollider2D _circleCollider2D;

        private CircleCollider2D CircleCollider2D => _circleCollider2D == null
            ? (_circleCollider2D = GetComponent<CircleCollider2D>())
            : _circleCollider2D;

        public UnityEvent<DynamicBlock> onDynamicBlockCollected;

        protected override bool HasTarget(Rigidbody2D rbTarget, out DynamicBlock value)
        {
            if (rbTarget.CompareTag(targetTag))
            {
                value = rbTarget.gameObject.GetComponent<DynamicBlock>();
                return true;
            }

            value = null;
            return false;
        }
        protected override bool HasTarget(Collider2D target, out DynamicBlock value)
        {
            if (target.CompareTag(targetTag))
            {
                value = target.gameObject.GetComponent<DynamicBlock>();
                return true;
            }

            value = null;
            return false;
        }

        protected override void OnTargetAdded(DynamicBlock target, int totalNumberOfTargets)
        {
            if(target != null)
                onDynamicBlockCollected?.Invoke(target);
            base.OnTargetAdded(target, totalNumberOfTargets);
        }
        
        
    }
    
    
    
}