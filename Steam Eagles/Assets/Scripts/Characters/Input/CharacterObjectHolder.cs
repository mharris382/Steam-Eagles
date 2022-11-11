using StateMachine;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public class CharacterObjectHolder : MonoBehaviour
    {
        public TriggerArea HoldTrigger;
        public SharedTransform CharacterTransform;
        public Joint2D holdPoint;
        public Rigidbody2D heldObject;

        public float pickupDist;
        public UnityEvent<Rigidbody2D> OnHeld;

        public float minHoldDistance = 1; 
        public float holdResetTime = 1;

        private float lastGrabTime;
        private CharacterInputState _characterInputState;
        
        
        private void Awake()
        {
            HoldTrigger.onTargetAdded.AddListener(HoldTarget);

        }

        bool CanGrab(Rigidbody2D rb)
        {
            if(Time.time- lastGrabTime < holdResetTime)
                return false;
            if (heldObject == null)
                return true;
            return false;
        }

        

        void HoldTarget(Rigidbody2D rb)
        {
            if (CanGrab(rb))
            {
                Grab(rb);
            }
        }
        
        

        private void Grab(Rigidbody2D rb)
        {
            var dist = Vector2.Distance(rb.position, this.holdPoint.transform.position);
            if (dist < minHoldDistance)
            {
                var dif = rb.position - (Vector2)this.holdPoint.transform.position;
                rb.position = (Vector2)this.holdPoint.transform.position + dif.normalized * minHoldDistance;
            }
            
            holdPoint.connectedBody = rb;
            heldObject = rb;
            
            OnHeld?.Invoke(rb);
            SetCollidersEnabled(rb, false);
        }

        private static void SetCollidersEnabled(Rigidbody2D rb, bool enabled)
        {
            var colls = new Collider2D[rb.attachedColliderCount];
            rb.GetAttachedColliders(colls);
            foreach (var c in colls)
            {
                c.enabled = enabled;
            }
        }
    }
}