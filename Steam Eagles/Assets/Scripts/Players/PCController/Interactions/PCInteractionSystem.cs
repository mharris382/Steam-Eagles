using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Interactions;
using Players.PCController.ParallaxSystems;
using UniRx;
using UnityEngine;
using Zenject;

namespace Players.PCController.Interactions
{
    
    
    public class PCInteractionSystem : PCSystem, ITickable, ILateTickable
    {
        private IDisposable _disposable;
        IInteractable _lastInteractable;
        
        
        private readonly InteractionCameras _cameras;
        private Dictionary<IInteractable, VCam> _seenVCams = new Dictionary<IInteractable, VCam>();
        private readonly ReactiveProperty<IInteractable> _currentInteractable = new ReactiveProperty<IInteractable>();

        private readonly InteractionAgent _agent;
        private readonly CharacterInteractionState _characterInteractionState;
        private readonly IAvailableInteractionLabel _interactionLabel;

        public PCInteractionSystem(PC pc, InteractionCameras cameras) : base(pc)
        {
            _cameras = cameras;
            Debug.Log($"Created PCInteractionSystem for Player {pc.PlayerNumber}");

            _agent = pc.PCInstance.character.GetComponent<InteractionAgent>();
            _characterInteractionState = pc.PCInstance.character.GetComponent<CharacterInteractionState>();

            Debug.Assert(_agent != null, "InteractionAgent is null");
            Debug.Assert(_characterInteractionState != null, "CharacterInteractionState is null");

            _interactionLabel = _characterInteractionState.GetComponentInChildren<IAvailableInteractionLabel>();
            Debug.Assert(_interactionLabel != null, "InteractionLabel is null");
            _interactionLabel.SetText("");
            
            var cd = new CompositeDisposable();
            _currentInteractable.Subscribe(ChangeCurrentInteractable).AddTo(cd);
            _currentInteractable.Subscribe(UpdateInteractionCamera).AddTo(cd);
            _agent.SelectedInteractable.Select(t => _currentInteractable.Value == null ? t : null)
                .Subscribe(UpdateInteractionLabel);
            _disposable = cd;
        }


        private void UpdateInteractionCamera(IInteractable interactable)
        {
            DisableAllInteractionCameras();
            if (interactable != null && interactable.PCVirtualCamera != null)
            {
                if (!_seenVCams.TryGetValue(interactable, out var vcam))
                {
                    vcam = interactable.PCVirtualCamera.GetComponent<VCam>() ?? interactable.PCVirtualCamera.AddComponent<VCam>();
                    _seenVCams.Add(interactable, vcam);
                }
                vcam = _cameras.GetCopyForPlayer(vcam, Pc.PlayerNumber);
                vcam.enabled = true;
            }
        }

        private void UpdateInteractionLabel(Interactable interactable)
        {
            if (interactable == null)
                _interactionLabel.SetText("");
            else
                _interactionLabel.SetText(interactable.ControlHint);
        }

        private void DisableAllInteractionCameras()
        {
            foreach (var c in _cameras.GetCopies(Pc.PlayerNumber))
            {
                if (c.enabled)
                {
                    Debug.Log($"Disabled VCam {c.name} for player {Pc.PlayerNumber}", c);
                    c.enabled = false;
                }
            }
        }

        private void ChangeCurrentInteractable(IInteractable nextInteractable)
        {
            if (_lastInteractable != null)
            {
                if (_lastInteractable.PCVirtualCamera != null)
                {
                    VCam vcam;
                    if (!_lastInteractable.PCVirtualCamera.TryGetComponent(out vcam))
                    {
                        vcam = _lastInteractable.PCVirtualCamera.AddComponent<VCam>();
                    }
                    vcam = _cameras.GetCopyForPlayer(vcam, Pc.PlayerNumber);
                    vcam.enabled = false;
                    Debug.Log($"Disabled VCam {vcam.name} for player {Pc.PlayerNumber}", vcam);
                }
            }
            if (nextInteractable != null)
            {
                if (nextInteractable.PCVirtualCamera != null)
                {
                    VCam vcam;
                    if (!nextInteractable.PCVirtualCamera.TryGetComponent(out vcam))
                    {
                        vcam = nextInteractable.PCVirtualCamera.AddComponent<VCam>();
                    }
                    vcam = _cameras.GetCopyForPlayer(vcam, Pc.PlayerNumber);
                    vcam.enabled = true;
                    Debug.Log($"Enabled VCam {vcam.name} for player {Pc.PlayerNumber}", vcam);
                }
            }
            _lastInteractable = nextInteractable;
        }

        private bool HasResources()
        {
            if (_characterInteractionState == null)
            {
                return false;
            }
            if(_agent == null)
            {
                return false;
            }
            if(_cameras == null)
            {
                return false;
            }
            return true;
        }
        
        public void Tick()
        {
            if (HasResources() == false)
            {
                Debug.LogError("Missing resources for PCInteractionSystem");
                return;
            }
            
            UpdateAgentInputs();
            HandleInteractions();
        }

        private void UpdateAgentInputs()
        {
            _agent.InteractPressed = _characterInteractionState.Input.InteractPressed;
        }

        private void HandleInteractions()
        {
            _characterInteractionState.IsInteracting = _currentInteractable.Value != null;
            
            //character is currently interacting with something
            if (_characterInteractionState.IsInteracting)
            {
                HandleActiveInteraction(_currentInteractable.Value);
            }
            //character is not interacting with anything
            else
            {
                CheckForInteractionStart();
            }
        }

        private void CheckForInteractionStart()
        {
            _characterInteractionState.IsInteracting = false;
            if (_agent.HasInteractableInRange)
            {
                var selected = _agent.SelectedInteractable.Value;
                if (selected != null && _characterInteractionState.Input.InteractPressed)
                {
                    _currentInteractable.Value = selected;
                    Debug.Log($"Interacting with {selected.name}");
                }
            }
        }

        private void HandleActiveInteraction(IInteractable currentInteractableValue)
        {
            if (_characterInteractionState.Input.CancelPressed)
            {
                Debug.Log($"Stopped interacting with {currentInteractableValue.name}");
                _currentInteractable.Value = null;
            }
            
        }

        public void LateTick()
        {
            //complete job to determine which interactable is closest to the player
            //if interactable is close enough, display the interactable UI and notify state that there is an interaction available
            
        }

        public class Factory : PlaceholderFactory<PC, PCInteractionSystem>, ISystemFactory<PCInteractionSystem> { }
    }



   
}