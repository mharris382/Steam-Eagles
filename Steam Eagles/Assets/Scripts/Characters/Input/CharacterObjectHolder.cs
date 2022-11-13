﻿using System;
using System.Collections;
using Puzzles;
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

        [Tooltip("Character Root Transform")] 
        public SharedTransform CharacterTransform;

        public Joint2D holdPoint;


        public string tag = "Builder";
        public UnityEvent<Rigidbody2D> onHeld;

        [Header("Settings")]
        public float minHoldDistance = 1; 
        public float holdResetTime = 1;
        public float throwMultiplier = 15;

        
        
        
        
        
        private float _lastGrabTime;
        
        public Rigidbody2D heldRigidBody;
        private HoldableItem _held;
        private CharacterInputState _characterInputState;

        private IDisposable _disposable;
        private Collider2D[] _disabledColliders = new Collider2D[0];
        
        
        public Rigidbody2D HeldRigidBody
        {
            get => heldRigidBody;
            set
            {
                heldRigidBody = value;
                HeldItem = GetComponent<HoldableItem>();
            }
        }
        
        private HoldableItem HeldItem
        {
            get => _held;
            set
            {
                if (_held != value)
                {
                    var prev = _held;
                    _held = value;
                    if(prev!=null)prev.onDropped?.Invoke(Holder);
                    if(_held!=null)_held.onPickedUp?.Invoke(Holder);
                }

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
            HoldTrigger.onTargetAdded.AddListener(HoldTarget);
            
            CharacterTransform.onValueChanged.AsObservable()
                .Where(t => t != null)
                .Subscribe(t => {
                    if(_disposable != null)_disposable.Dispose();
                    _characterInputState = t.GetComponentInParent<CharacterInputState>();
                    if (_characterInputState != null)
                    {
                        Debug.Log($"Connecting Character Object Holder to Input State {_characterInputState.name}");
                        _disposable = _characterInputState.onPickup.AsObservable().Subscribe(OnPickupInput);
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

        private void OnPickupInput(InputAction.CallbackContext context)
        {
            Debug.Log("Pickup Event Occurred");
            if (HeldRigidBody != null)
            {
                ReleaseObject();
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

        private void HoldTarget(Rigidbody2D rb)
        {
            if (CanGrab(rb))
            {
                Grab(rb);
            }
        }

        private void ReleaseObject()
        {
            if(HeldRigidBody == null)
                return;
            Release(HeldRigidBody, Vector2.zero, Rand.Range(- 15f, 15f));
        }


        private void Release(Rigidbody2D rb, Vector2 releaseForce, float releaseTorque)
        {
            if(rb == null)
                return;
            
            HeldRigidBody = null;
            holdPoint.connectedBody = null;
            _characterInputState.SetHeldItem(null);
           
            GetComponent<HoldableItem>()?.onPickedUp?.Invoke(_characterInputState.gameObject);

            var force = releaseForce + (_characterInputState.MoveInput * throwMultiplier);
            var heldBy = _characterInputState.GetComponent<Rigidbody2D>();
            if (heldBy != null)
            {
                force +=heldBy.velocity;
                SetCollidersEnabled(rb, true);
                StartCoroutine(PassthroughPlayerOnThrow(rb, heldBy));
            }
            else
            {
                SetCollidersEnabled(rb, true);
            }
            
            if(force != Vector2.zero) rb.AddForce(force, ForceMode2D.Impulse);
            if(releaseTorque != 0) rb.AddTorque(releaseTorque, ForceMode2D.Impulse);
        }

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
            
            for (int i = 0; i < rb.attachedColliderCount; i++) Physics2D.IgnoreCollision(capColl, colls[i], false);
        }

        private void Grab(Rigidbody2D rb)
        {
            CheckDistance(rb);
            
            holdPoint.connectedBody = rb;
            HeldRigidBody = rb;
            SetCollidersEnabled(rb, false);
            _characterInputState.SetHeldItem(HeldRigidBody);
            GetComponent<HoldableItem>()?.onPickedUp?.Invoke(_characterInputState.gameObject);
            onHeld?.Invoke(rb);
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