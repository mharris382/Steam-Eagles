using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    public class InteractionDisplay : MonoBehaviour
    {
       

        public UnityEvent onNoInteractionAvailable;
        public UnityEvent onInteractionAvailable;
        
        //public UnityEvent<string> inputLabel;
        public UnityEvent<string> interactionLabel;
        public Transform display;

        private void Awake()
        {
            var controller = GetComponentInParent<InteractionController>();
            if (controller == null)
            {
                return;
            }

            controller._currentAvailableInteractable.Where(t => t == null).Subscribe(_ =>
            {
                onNoInteractionAvailable?.Invoke();
            }).AddTo(this);
            
            controller._currentAvailableInteractable.Where(t => t != null).Subscribe(t =>
            {
                onInteractionAvailable?.Invoke();
                //inputLabel?.Invoke(controller.inputButton);
                interactionLabel?.Invoke(t.description);
            }).AddTo(this);
        }
    }
}