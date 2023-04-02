using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    [System.Obsolete("Interaction System will be replaced with a generic version to be used by both NPC (AI) and PC (players)")]
    public class InteractableObject : MonoBehaviour
    {
        public Vector2 Position => transform.position;
        [SerializeField]
        private string description = "Flip Lever";
        public InteractionType interactionType;
        public UnityEvent onInteraction;
        
        public enum InteractionType
        {
            INSTANT,
            ANALOG
        }
        
        public virtual string Description => description;
        
        private Subject<string> _onDescriptionChange = new Subject<string>();
        
        public IObservable<string> OnDescriptionChange => _onDescriptionChange;

        public bool inUse;
        private void Awake()
        {
            InteractionManager.Instance.interactableObjects.Add(this);
        }

        public virtual void OnInteractionTriggered(InteractionController actor)
        {
            onInteraction?.Invoke();   
        }
        
        
        public void NotifyDescriptionChange()
        {
            _onDescriptionChange.OnNext(Description);
            MessageBroker.Default.Publish(new InteractionLabelChanged()
            {
                interactable = this
            });
        }
        
    }
}