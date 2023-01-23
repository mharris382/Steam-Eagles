using System;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    public class LeverInteractableObject : InteractableObject
    {
        public override string Description
        {
            get => _resetting ? "" : leverInteractions[_currentInteractionIndex].description;
        }

        [Serializable]
        private class LeverInteraction
        {
            public string description = "Flip Lever";
            public UnityEvent onInteraction;
        }
        
        [SerializeField]
        private LeverInteraction[] leverInteractions;
        private int _currentInteractionIndex = 0;
        private bool _resetting = false;

        public override void OnInteractionTriggered(InteractionController actor)
        {
            if (_resetting) return;
            StartResetTimer();
            var interaction = leverInteractions[_currentInteractionIndex];
            interaction.onInteraction?.Invoke();
            _currentInteractionIndex += 1;
            if (_currentInteractionIndex >= leverInteractions.Length) _currentInteractionIndex = 0;
        }

        void StartResetTimer()
        {
            _resetting = true;
            Invoke(nameof(ResetTimer), 5f);
        }
        void ResetTimer()
        {
            _resetting = false;
        }
    }
}