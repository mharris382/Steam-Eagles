using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Interactables
{
    [System.Obsolete("Interaction System will be replaced with a generic version to be used by both NPC (AI) and PC (players)")]
    public class LeverInteractableObject : InteractableObject
    {
        public override string Description
        {
            get
            {
                if (!toggleMode)
                {
                    return _resetting ? "" : leverInteractions[_currentInteractionIndex].description;
                }
                else
                {
                    return _state ? offDescription : onDescription;
                }
            }
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

        
        [ToggleGroup(nameof(toggleMode))] public bool toggleMode = false;

        
       [ToggleGroup(nameof(toggleMode))] public Animator toggledAnimator;
       [ToggleGroup(nameof(toggleMode))] public bool startToggled = false;
        private bool _state;
        [ToggleGroup(nameof(toggleMode))] public string onDescription = "On";
        [ToggleGroup(nameof(toggleMode))] public UnityEvent onToggleOn;
        
        [ToggleGroup(nameof(toggleMode))]public string offDescription = "Off";
        [ToggleGroup(nameof(toggleMode))]public UnityEvent onToggleOff;
        
        void Start()
        {
            _state = startToggled;
            if(toggledAnimator)
                toggledAnimator.SetBool("On", _state);
        }

        public override void OnInteractionTriggered(InteractionController actor)
        {
            if (toggleMode)
            {
                _state = !_state;
                if(toggledAnimator)
                    toggledAnimator.SetBool("On", _state);
                if (_state)
                {
                    onToggleOn?.Invoke();
                }
                else
                {
                    onToggleOff?.Invoke();
                }
                NotifyDescriptionChange();
            }
            else
            {
                if (_resetting) return;
                StartResetTimer();
                var interaction = leverInteractions[_currentInteractionIndex];
                interaction.onInteraction?.Invoke();
                _currentInteractionIndex += 1;
                if (_currentInteractionIndex >= leverInteractions.Length) _currentInteractionIndex = 0;
            }
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