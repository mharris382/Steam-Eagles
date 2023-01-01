using System;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    
    public class InteractableObject : MonoBehaviour
    {
        public Vector2 Position => transform.position;
        
        public string description = "Flip Lever";
        public InteractionType interactionType;
        public UnityEvent onInteraction;
        
        public enum InteractionType
        {
            INSTANT,
            ANALOG
        }
        
        

        public bool inUse;
        private void Awake()
        {
            InteractionManager.Instance.interactableObjects.Add(this);
        }

        public virtual void OnInteractionTriggered(InteractionController actor)
        {
            onInteraction?.Invoke();   
        }
        
        
    }
}