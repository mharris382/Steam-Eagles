using System;
using System.Collections;
using CoreLib;
using CoreLib.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    
    /// <summary>
    /// component responsible for the PlayerInput side of the PlayerController, this component will be attached to the PlayerInput GameObject
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputWrapper : MonoBehaviour
    {
        
        
        public PlayerInput PlayerInput => _playerInput == null ? (_playerInput = GetComponent<PlayerInput>()) : _playerInput;

        
        private PlayerInput _playerInput;
        private CharacterInputState _characterInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            PlayerInput.onDeviceLost += input =>
            {
                Debug.Log("Device Lost");
            };
            MessageBroker.Default.Receive<GameLoadComplete>().AsUnitObservable()
                .Subscribe(_ =>
                {
                    PlayerInput.SwitchCurrentActionMap("Gameplay");
                }).AddTo(this);
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
            if (_characterInput == null)
            {
                Debug.LogWarning("CharacterInputState is null", this);
                return;
            }
            //_characterInput.MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
            //_characterInput.AimInput = PlayerInput.actions["Aim"].ReadValue<Vector2>();
           //// _characterInput.DropHeldItem = PlayerInput.actions["Interact"].IsPressed();
            //_characterInput.JumpPressed = PlayerInput.actions["Jump"].WasPressedThisFrame();
            //_characterInput.JumpHeld = PlayerInput.actions["Jump"].IsPressed();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            MessageBroker.Default.Publish(new JumpActionEvent()
            {
                context = context,
                tag = _characterInput.gameObject.tag,
                characterState = _characterInput.CharacterState,
                transform = _characterInput.transform
            });
            //_characterInput.JumpPressed = context.performed;
            //_characterInput.JumpHeld = context.started;
        }
        
        public void OnAim(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            //_characterInput.AimInput = context.ReadValue<Vector2>();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            //_characterInput.MoveInput = context.ReadValue<Vector2>();
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            //_characterInput.onInteract?.Invoke(context);
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
            //_characterInput.onPickup?.Invoke(context);
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
            //_characterInput.onValve?.Invoke(context);
            MessageBroker.Default.Publish(new ValveActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform
            });
        }

        public void OnAbility01(InputAction.CallbackContext context)
        {
            var eventInfo = new UseAbilityActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                inputState = this._characterInput,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform,
                abilityID =PlayerAbilityID.PRIMARY,
                
            };
            MessageBroker.Default.Publish(eventInfo);
        }

        public void OnAbility02(InputAction.CallbackContext context)
        {
            var eventInfo = new UseAbilityActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                inputState = this._characterInput,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform,
                abilityID =PlayerAbilityID.SECONDARY
            };
            MessageBroker.Default.Publish(eventInfo);
        }
        
        public void SelectNextAbility(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            var eventInfo = new SelectAbilityActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform,
                abilityIndex = 1,
                isRelative =  true
            };
            MessageBroker.Default.Publish(eventInfo);
        }
        public void SelectPreviousAbility(InputAction.CallbackContext context)
        {
            if (_characterInput == null) return;
            var eventInfo = new SelectAbilityActionEvent()
            {
                context = context,
                characterState = this._characterInput.CharacterState,
                tag = _characterInput.gameObject.tag,
                transform = _characterInput.transform,
                abilityIndex = -1,
                isRelative =  true
            };
            MessageBroker.Default.Publish(eventInfo);
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