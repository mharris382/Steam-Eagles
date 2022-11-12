using System;
using System.Collections;
using StateMachine;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Observable = UnityEngine.InputSystem.Utilities.Observable;
using Rand = UnityEngine.Random;

namespace Characters
{
    public class CharacterObjectHolder : MonoBehaviour
    {
        public TriggerArea HoldTrigger;
        public SharedTransform CharacterTransform;
        public Joint2D holdPoint;
        public Rigidbody2D heldObject;

        
        public UnityEvent<Rigidbody2D> OnHeld;

        public float minHoldDistance = 1; 
        public float holdResetTime = 1;

        private float lastGrabTime;
        private CharacterInputState _characterInputState;

        private IDisposable d;
        public string tag = "Builder";
        void TrySwitchCharacters(Transform t)
        {
            
        }

       
        private void Awake()
        {
            HoldTrigger.onTargetAdded.AddListener(HoldTarget);
            
            CharacterTransform.onValueChanged.AsObservable()
                .Where(t => t != null)
                .Subscribe(t =>
                {
                    //transform.parent = t;
                    //transform.parent = t;
                    if(d != null)d.Dispose();
                    _characterInputState = t.GetComponentInParent<CharacterInputState>();
                    if (_characterInputState != null)
                    {
                        Debug.Log($"Connecting Character Object Holder to Input State {_characterInputState.name}");
                        d = _characterInputState.onPickup.AsObservable().Subscribe(OnPickupInput);
                    }
                });

            MessageBroker.Default.Receive<PickupActionEvent>()
                .Where(t => t.tag == this.tag)
                .Select(t => t.context)
                .Subscribe(OnPickupInput);
        }

        private IEnumerator Start()
        {
            while (CharacterTransform.Value == null)
            {
                yield return null;
            }
            while(_characterInputState == null && CharacterTransform.Value != null)
            {
                
                _characterInputState = CharacterTransform.Value.GetComponentInParent<CharacterInputState>();
                Debug.Assert(_characterInputState != null, $"Missing Character Input State on {CharacterTransform.Value.name}", CharacterTransform.Value);
                yield return null;
            }
            
        }

        void OnPickupInput(InputAction.CallbackContext context)
        {
            Debug.Log("Pickup Event Occurred");
            if (heldObject != null)
            {
                ReleaseObject();
            }
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

        void ReleaseObject()
        {
            if(heldObject == null)
                return;
            Release(heldObject, Vector2.zero, Rand.Range(- 15f, 15f));
        }

        private Collider2D[] _disabledColliders = new Collider2D[0];

        private void Release(Rigidbody2D rb, Vector2 releaseForce, float releaseTorque)
        {
            if(rb == null)
                return;
            
            heldObject = null;
            holdPoint.connectedBody = null;
            SetCollidersEnabled(rb, true);
            if(releaseForce != Vector2.zero)
                rb.AddForce(releaseForce, ForceMode2D.Impulse);
            
            if(releaseTorque != 0)
                rb.AddTorque(releaseTorque, ForceMode2D.Impulse);
        }

        private void Grab(Rigidbody2D rb)
        {
            CheckDistance(rb);
            
            holdPoint.connectedBody = rb;
            heldObject = rb;
            SetCollidersEnabled(rb, false);
            
            OnHeld?.Invoke(rb);
        }

        private void CheckDistance(Rigidbody2D rb)
        {
            var dist = Vector2.Distance(rb.position, this.holdPoint.transform.position);
            if (dist < minHoldDistance)
            {
                var dif = rb.position - (Vector2)this.holdPoint.transform.position;
                rb.position = (Vector2)this.holdPoint.transform.position + dif.normalized * minHoldDistance;
            }
        }

        private  void SetCollidersEnabled(Rigidbody2D rb, bool enabled)
        {
            if (enabled == true)
            {
                foreach (var disabledCollider in _disabledColliders)
                {
                    disabledCollider.enabled = true;
                }
            }
            else
            {
                var colls = new Collider2D[rb.attachedColliderCount];
                rb.GetAttachedColliders(colls);
                foreach (var c in colls)
                {
                    c.enabled = enabled;
                }
                _disabledColliders = colls;
            }
          


        }
    }
}