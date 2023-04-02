using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    [System.Obsolete("Interaction System will be replaced with a generic version to be used by both NPC (AI) and PC (players)")]
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
                interactionLabel?.Invoke(t.Description);
            }).AddTo(this);
       
            MessageBroker.Default.Receive<InteractionLabelChanged>().Subscribe(t =>
            {
                if (t.interactable == controller._currentAvailableInteractable.Value)
                {
                    interactionLabel?.Invoke(controller._currentAvailableInteractable.Value.Description);
                }
            }).AddTo(this);
        }
    }
}