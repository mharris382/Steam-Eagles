using System;
using System.Collections;
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
        
        
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput == null ? (_playerInput = GetComponent<PlayerInput>()) : _playerInput;

        public bool useEventsForJump;
        
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
            enabled = true;
        }

        public void UnAssign(CharacterInputState characterInputState)
        {
            if (characterInputState == _characterInput)
            {
                _characterInput = null;
                enabled = false;
            }
        }


        private void Update()
        {
            if (_characterInput == null) return;
            _characterInput.MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
            _characterInput.AimInput = PlayerInput.actions["Aim"].ReadValue<Vector2>();
            _characterInput.DropHeldItem = PlayerInput.actions["Interact"].IsPressed();
            if (useEventsForJump) return;
            _characterInput.JumpPressed = PlayerInput.actions["Jump"].WasPressedThisFrame();
            _characterInput.JumpHeld = PlayerInput.actions["Jump"].IsPressed();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (_characterInput == null || !useEventsForJump) return;
            _characterInput.JumpPressed = context.performed;
            _characterInput.JumpHeld = context.started;
        }
    }
}