using System;
using Buildings;
using CoreLib;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interactions
{
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        public abstract UniTask<bool> Interact(InteractionAgent agent);

        public float range = 1f;
        
        public GameObject virtualCamera;
        [SerializeField] string controlHint;

        public string ControlHint => string.IsNullOrEmpty(controlHint) ? name : controlHint;
        
        private void OnEnable()
        {
            InteractionManager.Instance.RegisterInteractable(this);
            if(virtualCamera)
                virtualCamera.SetActive(false);
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

        public GameObject PCVirtualCamera => virtualCamera;
    }


    
}