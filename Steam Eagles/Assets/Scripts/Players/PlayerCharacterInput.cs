using System;
using System.Collections;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    
    /// <summary>
    /// component responsible for the PlayerInput side of the PlayerController, this component will be attached to the PlayerInput GameObject
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCharacterInput : MonoBehaviour
    {
        
        
        public PlayerInput PlayerInput => _playerInput == null ? (_playerInput = GetComponent<PlayerInput>()) : _playerInput;

        
        private PlayerInput _playerInput;
        private CharacterInputState _characterInput;

        private void Awake()
        {
            PlayerInput.onDeviceLost += input =>
            {
                Debug.Log("Device Lost");
            };
        }

        public void Assign(CharacterInputState characterInputState)
        {
            if (characterInputState == null) return;
            _characterInput = characterInputState;
            _characterInput.AssignPlayer(PlayerInput);
            enabled = true;
            this.name = $"PlayerInput {characterInputState.name}";
        }

        public void UnAssign(CharacterInputState characterInputState)
        {
            if (characterInputState == _characterInput)
            {
                _characterInput.UnAssignPlayer();
                enabled = false;
                this.name = $"PlayerInput (Unassigned)";
            }
        }


        private void Update()
        {
            if (_characterInput == null) return;
            _characterInput.MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
            _characterInput.AimInput = PlayerInput.actions["Aim"].ReadValue<Vector2>();
           // _characterInput.DropHeldItem = PlayerInput.actions["Interact"].IsPressed();
            _characterInput.JumpPressed = PlayerInput.actions["Jump"].WasPressedThisFrame();
            _characterInput.JumpHeld = PlayerInput.actions["Jump"].IsPressed();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            _characterInput.JumpPressed = context.performed;
            _characterInput.JumpHeld = context.started;
        }
        
        public void OnAim(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            _characterInput.AimInput = context.ReadValue<Vector2>();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            _characterInput.MoveInput = context.ReadValue<Vector2>();
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            _characterInput.onInteract?.Invoke(context);
            MessageBroker.Default.Publish(new InteractActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform
            });
        }
        
        public void OnPickup(InputAction.CallbackContext context)
        {
            Debug.Log("OnPickup called");
            if (_characterInput == null) return;
            _characterInput.onPickup?.Invoke(context);
            MessageBroker.Default.Publish(new PickupActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform
            });
        }
        
        public void OnValve(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            _characterInput.onValve?.Invoke(context);
            MessageBroker.Default.Publish(new ValveActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform
            });
        }
    }

    
    [RequireComponent(typeof(PlayerInput))]
    public class PauseMenuInput : MonoBehaviour
    {
        public SharedBool isPaused;
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput ==null ? (_playerInput = GetComponent<PlayerInput>()) : _playerInput;
        public void OnPause(InputAction.CallbackContext context)
        {
            
        }


        private void Awake()
        {
            isPaused.onValueChanged.AsObservable().Where(t=>t).Subscribe(_ =>
            {
                PlayerInput.SwitchCurrentActionMap("UI");
            }).AddTo(this);
            isPaused.onValueChanged.AsObservable().Where(t=>!t).Subscribe(_ =>
            {
                PlayerInput.SwitchCurrentActionMap("Player");
            }).AddTo(this);
        }
    }
}