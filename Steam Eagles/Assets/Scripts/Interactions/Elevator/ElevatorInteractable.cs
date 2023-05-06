using System;
using Characters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorInteractable : Interactable
    {
        private InteractionHandle _currentInteractionHandle;
        private InteractionHandle.Factory _interactionFactory;

        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            _currentInteractionHandle ??= _interactionFactory.Create(agent);
            await UniTask.WaitUntil(() => _currentInteractionHandle.WasInteractionCompleted);
            _currentInteractionHandle.Dispose();
            var result = !_currentInteractionHandle.WasInteractionCancelled;
            _currentInteractionHandle = null;
            return result;
        }

        [Inject] public void InjectMe(InteractionHandle.Factory interactionFactory)
        {
            _interactionFactory = interactionFactory;
        }


        private void Update()
        {
            if (_currentInteractionHandle != null)
            {
                _currentInteractionHandle.Update();
            }
        }

        public class InteractionHandle : IDisposable
        {
            public class Factory : PlaceholderFactory<InteractionAgent, InteractionHandle>{ }

            private readonly ElevatorController _elevatorController;

            private readonly CharacterInteractionState _characterInteractionAgent;

            public bool WasInteractionCancelled { get; private set; }
            public bool WasInteractionCompleted { get; private set; }
            
            
            public InteractionHandle(InteractionAgent agent, ElevatorController elevatorController)
            {
                _elevatorController = elevatorController;
                _characterInteractionAgent = agent.GetComponent<CharacterInteractionState>();
                WasInteractionCancelled = false;
                WasInteractionCompleted = false;
                _characterInteractionAgent.Input.InteractPressed = false;
            }


            internal void Update()
            {
                if (WasInteractionCancelled || WasInteractionCompleted)
                {
                    Debug.LogWarning("Updating Completed or Cancelled Interaction!");
                    return;
                }
                
                if (_characterInteractionAgent.Input.CancelPressed)
                {
                    CancelInteraction(); 
                    return;
                }

                if (_characterInteractionAgent.Input.InteractPressed)
                {
                    CompleteInteraction();
                }
            }

            private void CompleteInteraction()
            {
                WasInteractionCompleted = true;
            }

            public void CancelInteraction()
            {
                WasInteractionCancelled = true;
            }

            public void Dispose()
            {
            }
        }
    }
}