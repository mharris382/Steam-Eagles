using System;
using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class PlayerTriggerBase : MonoBehaviour
    {
        protected bool IsPlayerDetected => _playerColliders.Count > 0;
        HashSet<Collider2D> _playerColliders = new HashSet<Collider2D>();

        private void Awake()
        {
            var colliders = GetComponents<Collider2D>();
            foreach (var c in colliders)
                if (c.isTrigger) return;
            Debug.LogError("A Player Trigger must have at least one Collider2D isTrigger enabled!",this);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) return;
            var go = other.attachedRigidbody.gameObject;
            if (go.CompareTag("Player")) 
            {
                if(!IsPlayerDetected) OnPlayerEnter(go);
                _playerColliders.Add(other);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) return;
            if (_playerColliders.Contains(other))
            {
                _playerColliders.Remove(other);
                if(!IsPlayerDetected) OnPlayerExit(other.attachedRigidbody.gameObject);
            }
        }

        protected abstract void OnPlayerEnter(GameObject player);
        protected abstract void OnPlayerExit(GameObject player);
    }
}