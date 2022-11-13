using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    /// <summary>
    /// listens for Valve input and a valve is available
    /// </summary>
    public class CharacterValveInputController : MonoBehaviour, ICharacterInput
    {
        private PlayerInput _input;
        private CharacterState _inputState;
        private IDisposable _inputDisposable;
        private Valve _activeValve;

        private Valve ActiveValve
        {
            get => _activeValve;
            set
            {
                if (value != _activeValve)
                {
                    Debug.Log($"Active valve changed to {value}");
                    _activeValve = value;
                    enabled = value != null;
                }
            }
        }
        
        private void Awake()
        {
            _inputState = GetComponentInParent<CharacterState>();
            Debug.Assert(_inputState != null, "Character Valve Input Controller needs a CharacterInputState", this);
            
            _inputState.HeldObject.Select(t => t == null ? null : t.GetComponent<Valve>()).TakeUntilDestroy(this)
                .Subscribe(valve => ActiveValve = valve);
        }
        
        public void AssignPlayer(PlayerInput playerInput)
        {
            Debug.Log("Character Valve Input Controller assigned player input", this);
            _input = playerInput;
            enabled = false;
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