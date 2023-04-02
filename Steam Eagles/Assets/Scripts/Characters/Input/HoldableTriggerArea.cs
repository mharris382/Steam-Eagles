using UniRx;
using UnityEngine;

namespace Characters
{
    [System.Obsolete("This will be replaced with the inventory/tool system")]
    public class HoldableTriggerArea : TriggerArea
    {
        public ReactiveCollection<Rigidbody2D> availablePickups = new ReactiveCollection<Rigidbody2D>();
        

        protected override void OnTargetAdded(Rigidbody2D target, int totalNumberOfTargets)
        {
            base.OnTargetAdded(target, totalNumberOfTargets);
            availablePickups.Add(target);
        }

        protected override void OnTargetRemoved(Rigidbody2D target, int totalNumberOfTargets)
        {
            base.OnTargetRemoved(target, totalNumberOfTargets);
            availablePickups.Remove(target);
        }
    }
}