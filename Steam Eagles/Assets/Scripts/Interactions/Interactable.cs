using System;
using Buildings;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract UniTask<bool> Interact(InteractionAgent agent);

        public float range = 1f;
        private void OnEnable()
        {
            InteractionManager.Instance.RegisterInteractable(this);
        }
        
        private void OnDisable()
        {
            InteractionManager.Instance.UnregisterInteractable(this);
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}