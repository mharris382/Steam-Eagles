using System;
using System.Collections;
using Puzzles;
using StateMachine;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Observable = UnityEngine.InputSystem.Utilities.Observable;
using Rand = UnityEngine.Random;

namespace Characters
{
    public class CharacterObjectHolder : MonoBehaviour
    {
        [Serializable]
        public class Events
        {
            public UnityEvent<Rigidbody2D> onHeld;
            public UnityEvent<HoldableItem> onItemPickedUp;
            public UnityEvent<HoldableItem> onItemDropped;
        }
        
        [FormerlySerializedAs("HoldTrigger")] public TriggerArea holdTrigger;

        [Tooltip("Character Root Transform")] 
        public SharedTransform CharacterTransform;

        public Joint2D holdPoint;


        public string tag = "Builder";
        
        public Events events;
        
        [Header("Settings")]
        public float minHoldDistance = 1; 
        public float holdResetTime = 1;
        
        [Header("Throw Force")]
        public float throwMultiplier = 15;
        

        [Header("Throw Torque")]
        [MinMaxRange(-360, 360)]
        public Vector2 throwTorqueRange = new Vector2(-15, 15);
        public float throwTorqueMultiplier = 1;
        
       
        [Header("Attach Points")]
        public string targetTag = "Attach Point";

        public LayerMask checkLayer = 1 << 0;
        public float checkRadius = 1;
        
        
        private float _lastGrabTime;
        
        public Rigidbody2D heldRigidBody;
        private HoldableItem _held;
        private CharacterInputState _characterInputState;

        private IDisposable _disposable;
        private Collider2D[] _disabledColliders = new Collider2D[0];
        private IDisposable _holdDisposable;
        
        private bool _releasePressed;
        private bool _isReleasing;

        public bool HasHeldItem => HeldItem != null;

        public Rigidbody2D HeldRigidBody
        {
            get => heldRigidBody;
            set
            {
                heldRigidBody = value;
                HeldItem = value == null ? null : value.GetComponent<HoldableItem>();
            }
        }
        
        private HoldableItem HeldItem
        {
            get => _held==null && HeldRigidBody!=null ? (_held = HeldRigidBody.GetComponent<HoldableItem>()) : _held;
            set
            {
                _held = value;
            }
        }

        public GameObject Holder
        {
            get
            {
                if (_characterInputState != null) return _characterInputState.gameObject;
                if (CharacterTransform != null && CharacterTransform.HasValue) return CharacterTransform.Value.gameObject;
                Debug.LogWarning("CharacterObjectHolder has no character GameObject", this);
                return gameObject;
            }
        }
        
       
        private void Awake()
        {
            holdTrigger.onTargetAdded.AddListener(HoldTarget);
        
            MessageBroker.Default.Receive<PickupActionEvent>()
                .Where(t => t.tag == this.tag)
                .Select(t => t.context)
                .Subscribe(_ => _releasePressed = true).AddTo(this);
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
            _characterInputState.onPickup.AsObservable().Subscribe(_ => _releasePressed = true).AddTo(this);
        }
        

        private void Update()
        {
            
            if (_releasePressed && HasHeldItem)
            {
                _releasePressed = false;
                if (_isReleasing)
                {
                    Debug.Log("Already releasing Held item",this);
                    return;
                }
                _isReleasing = true;
                 StartCoroutine(ReleaseHeldObject());
            }else if (_releasePressed && !HasHeldItem)
            {
                _releasePressed = false;
                
            }
        }

     

        #region [Drop/Release Methods]

        IEnumerator DoReleaseDelay()
        {
            var colliders = recentlyHeld.GetComponents<Collider2D>();
            var holderBody = Holder.GetComponent<Rigidbody2D>();
            var holderCollider = holderBody.gameObject.GetComponent<CapsuleCollider2D>();
            foreach (var collider2D1 in colliders)
            {
                Physics2D.IgnoreCollision(holderCollider, collider2D1, true);
            }
            yield return new WaitForSeconds(1);
            foreach (var collider2D1 in colliders)
            {
                Physics2D.IgnoreCollision(holderCollider, collider2D1, false);
            }
            recentlyHeld = null;
        }
        float _lastReleaseTime;
        private IEnumerator ReleaseHeldObject()
        {
            _isReleasing = true;
            
            var heldBody = heldRigidBody;
            var holderBody = Holder.GetComponent<Rigidbody2D>();
            var holderCollider = holderBody.gameObject.GetComponent<CapsuleCollider2D>();
            var heldItem = HeldItem;
            Debug.Assert(heldBody != null && holderBody != null && heldItem != null, $"Missing Components! \nHeld Rigidbody{heldBody}\nHolder Rigidbody{holderBody}\nItem:{heldItem}", this);
            recentlyHeld = heldBody;
            StartCoroutine(DoReleaseDelay());
            
            ClearHeld();
            DisconnectJoint();
            InvokeDropEvents();
            if (CheckForAttachPoints(heldBody, out var point))
            {
                var position = point.transform.TransformPoint(point.offset);
                var targetRotation = point.transform.eulerAngles.z;
                heldBody.position = position;
                heldBody.rotation = targetRotation;
                heldBody.velocity = Vector2.zero;
                if (point.gameObject.TryGetComponent<AttachPoint>(out var attachPoint))
                {
                    attachPoint.Attach(heldItem, heldBody, holderBody);
                }
                Debug.Log($"Found Attach Point {point.name}", point);
                Debug.Log($"Attaching {heldBody.name} to {point.name}", heldBody);
            }
            else
            {
                ApplyForces();    
            }
            
            
            foreach (var component in heldBody.GetComponents<Collider2D>())
            {
                component.enabled = true;
            }
            yield return PreventCollisionsWithPlayerOnThrow(holderCollider, heldItem.grabColliders);
            _lastGrabTime = Time.time;
            _isReleasing = false;
            EnsureDisconnect();
            void InvokeDropEvents()
            {
                heldItem.Dropped(holderBody.gameObject);
                events.onItemDropped?.Invoke(heldItem);
            }
            void DisconnectJoint()
            {
                holdPoint.connectedBody = null;
            }
            void ApplyForces()
            {
                var releaseForce = GetThrowForce(holderBody);
                var releaseTorque = GetThrowTorque(heldItem);
                if (releaseForce != Vector2.zero) heldBody.AddForce(releaseForce, ForceMode2D.Impulse);
                if (releaseTorque != 0) heldBody.AddTorque(releaseTorque, ForceMode2D.Impulse);
                heldItem.Thrown(releaseForce, releaseTorque);
            }
            void ClearHeld()
            {
                HeldRigidBody = null;
                _characterInputState.SetHeldItem(null);
                _holdDisposable?.Dispose();
                _holdDisposable = null;
            }
        }

        private void EnsureDisconnect()
        {
            holdTrigger.gameObject.GetComponent<Collider2D>().enabled = false;
            StartCoroutine(StayDisconnected());
        }

        private Rigidbody2D recentlyHeld;
        

        private IEnumerator StayDisconnected()
        {
            for (float i = 0; i < 1; i+=Time.deltaTime)
            {
                yield return null;
                if (holdPoint != null)
                {
                    holdPoint.enabled = false;
                }
            }

            holdPoint.enabled = true;
            holdTrigger.gameObject.GetComponent<Collider2D>().enabled = true;
        }

        /// <summary>
        /// check for nearby attach points, so that instead of throwing the object, it can be attached to the point 
        /// </summary>
        /// <param name="heldBody"></param>
        /// <param name="attachPoint"></param>
        /// <returns></returns>
        private bool CheckForAttachPoints(Rigidbody2D heldBody, out Collider2D attachPoint)
        {
            var position = heldBody.transform.position;
            var colliders = Physics2D.OverlapCircleAll(position, checkRadius, checkLayer);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    attachPoint = collider;
                    return true;
                }
            }
            attachPoint = null;
            return false;
        }
     
        private float GetThrowTorque(HoldableItem heldItem) => (Rand.Range(throwTorqueRange.x, throwTorqueRange.y) * throwTorqueMultiplier) * heldItem.TorqueMultiplier;


        private Vector2 GetThrowForce(Rigidbody2D heldBy)
        {
            if (_characterInputState != null)
            {
                var force = _characterInputState.MoveInput * throwMultiplier;
                    
                if (heldBy != null)
                {
                    force += heldBy.velocity;
                }
                return force;
            }
            return Vector2.zero;
        }


        private IEnumerator PreventCollisionsWithPlayerOnThrow(Collider2D playerCollider, Collider2D[] holdableColliders, float waitTime = 0.75f)
        {
             SetCollidersEnabled(holdableColliders, true);
            foreach (var holdableCollider in holdableColliders)
            {
                Physics2D.IgnoreCollision(playerCollider, holdableCollider, true);
            }
            yield return new WaitForSeconds(waitTime);
            foreach (var holdableCollider in holdableColliders)
            {
                Physics2D.IgnoreCollision(playerCollider, holdableCollider, false);
            }
        }
        
        #endregion


        #region [Pickup Methods]

        private void HoldTarget(Rigidbody2D rb)
        {
            if (CanGrab(rb))
            {
                Grab(rb);
                holdPoint.enabled = true;
            }
        }

        private bool CanGrab(Rigidbody2D rb)
        {
            if(recentlyHeld == rb)return false;
            if(Time.time- _lastGrabTime < holdResetTime)
                return false;
            if (HeldRigidBody != null)
                return false;
            if (rb.GetComponent<HoldableItem>() == null) 
                return false;
            return true;
        }

        private void Grab(Rigidbody2D rb)
        {
            CheckDistance(rb);
            
            holdPoint.connectedBody = rb;
            HeldRigidBody = rb;
            if (HeldItem == null)
            {
                HeldItem = rb.GetComponent<HoldableItem>();
                events.onItemPickedUp?.Invoke(HeldItem);
            }
            SetCollidersEnabled(rb, false);
            _characterInputState.SetHeldItem(HeldRigidBody);
            if(HeldItem!=null)
                HeldItem.PickedUp(_characterInputState.gameObject);
            
            events.onHeld?.Invoke(rb);
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

        #endregion
        private  void SetCollidersEnabled(Collider2D[] colls, bool enabled)
        {
            foreach (var c in colls)
            {
                c.enabled = enabled;
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
                    if (c.gameObject.layer == LayerMask.NameToLayer("TransparentFX")) continue;
                    c.enabled = enabled;
                }
                _disabledColliders = colls;
            }
          


        }
    }
}