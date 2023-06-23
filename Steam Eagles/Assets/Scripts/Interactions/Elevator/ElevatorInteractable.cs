using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorInteractable : Interactable
    {
        private InteractionHandle _currentInteractionHandle;
        private InteractionHandle.Factory _interactionFactory;
        private ElevatorController _controller;
        private IElevatorMechanism _mechanism;

        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            _currentInteractionHandle ??= _interactionFactory.Create(agent);
            await UniTask.WaitUntil(() => _currentInteractionHandle.WasInteractionCompleted || _currentInteractionHandle.WasInteractionCancelled);
            var result = !_currentInteractionHandle.WasInteractionCancelled;
            if (result)
            {
                _currentInteractionHandle.CurrentOption.OptionState = OptionState.CONFIRMED;
                _currentInteractionHandle.CurrentOption.Execute();
                await UniTask.WaitUntil(() => !_mechanism.IsMoving);
            }
            _currentInteractionHandle.Dispose();
            _currentInteractionHandle = null;
            return result;
        }

        private IEnumerable DisableSelectionOnElevatorArrive(ISelectableOption selectableOption)
        {
            while (_controller.IsMoving)
            {
                yield return null;
            }

            if (selectableOption.OptionState == OptionState.SELECTED)
                selectableOption.OptionState = OptionState.DEFAULT;
        }

        [Inject] public void InjectMe(InteractionHandle.Factory interactionFactory, ElevatorController controller, IElevatorMechanism mechanism)
        {
            _interactionFactory = interactionFactory;
            _controller = controller;
            _mechanism = mechanism;
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
            public ISelectableOption CurrentOption => _optionHandler.CurrentOption;
            public class Factory : PlaceholderFactory<InteractionAgent, InteractionHandle>{ }

            private readonly IElevatorMechanism _elevatorMechanism;
            private readonly ElevatorPassengers _passengers;
            private readonly ElevatorController _elevatorController;

            private readonly CharacterInteractionState _characterInteractionAgent;
            private readonly InteractionOptionHandler _optionHandler;

            private CompositeDisposable cd;
            public bool WasInteractionCancelled { get; private set; }
            public bool WasInteractionCompleted { get; private set; }

            private Subject<Unit> _update = new Subject<Unit>();
            public InteractionHandle(InteractionAgent agent, IElevatorMechanism elevatorMechanism, ElevatorPassengers passengers, ElevatorController elevatorController, InteractionOptionHandler.Factory optionHandlerFactory)
            {
                Rigidbody2D agentRigidbody = agent.GetComponent<Rigidbody2D>();
                Rigidbody2D elevatorRigidbody = elevatorController.GetComponent<Rigidbody2D>();
                
                
                Debug.Assert(agentRigidbody != null, "agentRigidbody != null");
                Debug.Assert(elevatorRigidbody != null, "elevatorRigidbody != null");

                _elevatorMechanism = elevatorMechanism;
                _passengers = passengers;
                _elevatorController = elevatorController;
                _optionHandler = optionHandlerFactory.Create(agent);
                _characterInteractionAgent = agent.GetComponent<CharacterInteractionState>();
                WasInteractionCancelled = false;
                WasInteractionCompleted = false;
                _characterInteractionAgent.Input.InteractPressed = false;
                cd = new CompositeDisposable();
                foreach (var passenger in passengers.Passengers)
                {
                    var fixedJoint = passenger.gameObject.AddComponent<FixedJoint2D>();
                    var localPoint = elevatorController.transform.InverseTransformPoint(passenger.position);
                    fixedJoint.connectedBody = elevatorRigidbody;
                    fixedJoint.autoConfigureConnectedAnchor = false;
                    List<Collider2D> colliders = new List<Collider2D>();
                    _update.Subscribe(_ => passenger.position = elevatorController.transform.TransformPoint(localPoint)).AddTo(cd);
                    Disposable.Create(() =>
                    {
                        Destroy(fixedJoint);
                        passenger.position = elevatorController.transform.TransformPoint(localPoint) + Vector3.up;
                        var capsule =passenger.GetComponent<CapsuleCollider2D>();
                        while (true)
                        {
                            int cnt = passenger.OverlapCollider(
                                new ContactFilter2D()
                                {
                                    useTriggers = false, useLayerMask = true,
                                    layerMask = LayerMask.GetMask("Ground", "Platforms")
                                }, colliders);
                            if(cnt == 0)
                                break;
                            bool checkAgain = false;
                            for (int i = 0; i < cnt; i++)
                            {
                                var collider = colliders[i];
                                if (collider == elevatorController.floorCollider)
                                {
                                    passenger.position += Vector2.up * 0.1f;
                                    checkAgain = true;
                                    break;
                                }
                            }
                            
                            if(!checkAgain)
                                break;
                        }
                        
                    }).AddTo(cd);
                }
            }


            internal void Update()
            {
                if (WasInteractionCancelled || WasInteractionCompleted)
                {
                    Debug.LogWarning("Updating Completed or Cancelled Interaction!");
                    return;
                }
                _optionHandler.Update();
                if(_elevatorMechanism.IsMoving)
                    _update.OnNext(Unit.Default);
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
                cd.Dispose();
                _update.Dispose();
            }
        }
    }
}