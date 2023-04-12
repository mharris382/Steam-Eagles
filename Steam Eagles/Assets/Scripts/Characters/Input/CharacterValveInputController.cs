using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [System.Obsolete("Early prototype type of gameplay interaction, should be replaced with concrete implementation of a tool ")]
    /// <summary>
    /// listens for Valve input and a valve is available
    /// </summary>
    public class CharacterValveInputController : MonoBehaviour, ICharacterInput
    {
        private PlayerInput _input;
        private Character _character;
        private IDisposable _inputDisposable;
        private Valve _activeValve;

        
        /// <summary>
        /// valve currently accepting player input
        /// </summary>
        private Valve ActiveValve
        {
            get => _activeValve;
            set
            {
                if (value != _activeValve)
                {
                    Debug.Log($"Active valve changed to {value}", this);
                    _activeValve = value;
                    enabled = value != null;
                }
            }
        }
        
        private void Awake()
        {
            _character = GetComponentInParent<Character>();
            Debug.Assert(_character != null, "Character Valve Input Controller needs a CharacterInputState", this);
            
            _character.HeldObject.Select(t => t == null ? null : t.GetComponent<Valve>()).TakeUntilDestroy(this)
                .Subscribe(valve => ActiveValve = valve);
            MessageBroker.Default.Receive<ValveActionEvent>().AsObservable()
                .Where(t => t.character == this._character)
                .Subscribe(t =>
                {
                    OnValve(t.context);
                });
        }

        public void AssignPlayer(PlayerInput playerInput)
        {
            Debug.Log("Character Valve Input Controller assigned player input", this);
            _input = playerInput;
            enabled = true;
            if (playerInput != null)
            {
                var id = _input.actions["Valve"].id;
                foreach (var inputActionEvent in _input.actionEvents)
                {
                    if (inputActionEvent.actionName == "Valve")
                    {
                        Debug.Log("Found Valve action event!", this);
                        _inputDisposable = inputActionEvent.AsObservable().Subscribe(context =>
                        {
                            Debug.Log("Valve action performed!", this);
                            OnValve(context);
                        });
                        break;
                    }
                }
            }
        }

        private void OnValve(InputAction.CallbackContext obj)
        {
            if (ActiveValve!= null)
            {
                var v = obj.ReadValue<float>();
                if (v > 0)
                {
                    ActiveValve.NudgeValve(Valve.ValveDirection.OPEN_PUSH);
                }
                else if(v < 0)
                {
                    ActiveValve.NudgeValve(Valve.ValveDirection.OPEN_PULL);
                }
            }
        }

        public void UnAssignPlayer()
        {
            _input = null;
            _inputDisposable?.Dispose();
            enabled = false;
        }

        private void Update()
        {
            if (_input == null)
                return;
            
        }
    }
}