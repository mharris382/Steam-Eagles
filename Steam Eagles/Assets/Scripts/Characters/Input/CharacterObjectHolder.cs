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
        
       
        
        
        
        private float _lastGrabTime;
        
        public Rigidbody2D heldRigidBody;
        private HoldableItem _held;
        private CharacterInputState _characterInputState;

        private IDisposable _disposable;
        private Collider2D[] _disabledColliders = new Collider2D[0];
        private IDisposable _holdDisposable;
        
        private bool _releasePressed;
        private bool _isReleasing;

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
            
          // CharacterTransform.onValueChanged.AsObservable()
          //     .Where(t => t != null)
          //     .Subscribe(t => {
          //         if(_disposable != null)_disposable.Dispose();
          //         _characterInputState = t.GetComponentInParent<CharacterInputState>();
          //         if (_characterInputState != null)
          //         {
          //             Debug.Log($"Connecting Character Object Holder to Input State {_characterInputState.name}");
          //             _disposable = _characterInputState.onPickup.AsObservable().Subscribe(OnPickupInput);
          //         }
          //     });

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
            
            if (_releasePressed)
            {
                _releasePressed = false;
                if (_isReleasing) return;
                _isReleasing = true;
                StartCoroutine(ReleaseHeldObject());
            }
        }

        private void OnPickupInput(InputAction.CallbackContext context)
        {
            Debug.Log("Pickup Event Occurred");
            if (context.canceled) return;
            if (HeldRigidBody != null)
            {
                ReleaseObject();
            }
        }

        #region [Drop/Release Methods]

        private IEnumerator ReleaseHeldObject()
        {
            _isReleasing = true;
            
            var heldBody = heldRigidBody;
            var holderBody = Holder.GetComponent<Rigidbody2D>();
            var holderCollider = holderBody.gameObject.GetComponent<CapsuleCollider2D>();
            var heldItem = HeldItem;
            Debug.Assert(heldBody != null && holderBody != null && heldItem != null, $"Missing Components! \nHeld Rigidbody{heldBody}\nHolder Rigidbody{holderBody}\nItem:{heldItem}", this);
            
            
            ClearHeld();
            DisconnectJoint();
            ApplyForces();
            InvokeDropEvents();
            foreach (var component in heldBody.GetComponents<Collider2D>())
            {
                component.enabled = true;
            }
            yield return PreventCollisionsWithPlayerOnThrow(holderCollider, heldItem.grabColliders);
            
            _isReleasing = false;

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
                var releaseTorque = GetThrowTorque();
                if (releaseForce != Vector2.zero) heldBody.AddForce(releaseForce, ForceMode2D.Impulse);
                if (releaseTorque != 0) heldBody.AddTorque(releaseTorque, ForceMode2D.Impulse);
            }
            void ClearHeld()
            {
                HeldRigidBody = null;
                _characterInputState.SetHeldItem(null);
                _holdDisposable?.Dispose();
                _holdDisposable = null;
            }
        }

        private void ReleaseObject()
        {
            if(HeldRigidBody == null)
                return;
            
            var heldBy = Holder.GetComponent<Rigidbody2D>();
            Release(HeldRigidBody, heldBy , GetThrowForce(heldBy), GetThrowTorque());
            
        }

        private float GetThrowTorque() => Rand.Range(throwTorqueRange.x, throwTorqueRange.y) * throwTorqueMultiplier;


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

        private void Release(Rigidbody2D rb, Rigidbody2D heldBy, Vector2 releaseForce, float releaseTorque)
        {
            if(rb == null)
                return;
            if (heldBy == null)
                return;
            
            SetCollidersEnabled(rb, true);
            
            if (HeldItem == null)
            {
                HeldItem = rb.GetComponent<HoldableItem>();
            }
            _characterInputState.StartCoroutine(PassthroughPlayerOnThrow(rb, heldBy));

            if (HeldItem != null)
            {
                HeldItem.Dropped(_characterInputState.gameObject);
                events.onItemDropped?.Invoke(HeldItem);
            }
            holdPoint.connectedBody = null;
            _characterInputState.SetHeldItem(null);
            HeldRigidBody = null;
            
            ApplyForces();

            void ApplyForces()
            {
                if (releaseForce != Vector2.zero) rb.AddForce(releaseForce, ForceMode2D.Impulse);
                if (releaseTorque != 0) rb.AddTorque(releaseTorque, ForceMode2D.Impulse);
            }
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
        
        /// <summary>
        /// this coroutine is necessary to prevent the thrown object from colliding with the player
        /// </summary>
        /// <param name="rb">thrown object</param>
        /// <param name="player">throwing character</param>
        /// <returns></returns>
        private IEnumerator PassthroughPlayerOnThrow(Rigidbody2D rb, Rigidbody2D player)
        {
            if (rb == null || player == null)
                yield break;
            
            var capColl = player.GetComponent<CapsuleCollider2D>();
            if (capColl == null) yield break;
            
            var colls = new Collider2D[rb.attachedColliderCount];
            rb.GetAttachedColliders(colls);
            
            for (int i = 0; i < rb.attachedColliderCount; i++) Physics2D.IgnoreCollision(capColl, colls[i], true);

            yield return new WaitForSeconds(0.75f);

            try
            {
                for (int i = 0; i < rb.attachedColliderCount; i++) Physics2D.IgnoreCollision(capColl, colls[i], false);
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        #endregion


        #region [Pickup Methods]

        private void HoldTarget(Rigidbody2D rb)
        {
            if (CanGrab(rb))
            {
                Grab(rb);
            }
        }

        private bool CanGrab(Rigidbody2D rb)
        {
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
                    c.enabled = enabled;
                }
                _disabledColliders = colls;
            }
          


        }
    }
}