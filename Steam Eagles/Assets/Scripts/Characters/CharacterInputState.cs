using System;
using Game;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Characters
{
    /// <summary>
    /// acts as a proxy for passing input to the player character
    /// </summary>
    [RequireComponent(typeof(CharacterState))]
    public class CharacterInputState : MonoBehaviour
    {
        
        
        [Serializable]
        public class DebugVariables
        {
            public bool hasPlayerAssigned;
            
            
            [NaughtyAttributes.HideIf(nameof(hasPlayerAssigned))][NaughtyAttributes.BoxGroup("Move")] public float xInput;
            [NaughtyAttributes.HideIf(nameof(hasPlayerAssigned))][NaughtyAttributes.BoxGroup("Move")] public float yInput;

            [NaughtyAttributes.HideIf(nameof(hasPlayerAssigned))][NaughtyAttributes.BoxGroup("Jump")] public bool jumpPressed;
            [NaughtyAttributes.HideIf(nameof(hasPlayerAssigned))][NaughtyAttributes.BoxGroup("Jump")] public bool jumpHeld;

            public void Update(CharacterState State)
            {
                xInput = State.MoveX;
                yInput = State.MoveY;
                jumpHeld = State.JumpHeld;
                jumpPressed = State.JumpPressed;
            }
        }
        
        [SerializeField] private DebugVariables debugVariables;
        [SerializeField] private bool useEventsForJump;
        
        private CharacterState _characterState;
        private PlayerInput _playerInput;


        
        public UnityEvent<InputAction.CallbackContext> onJump = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onInteract = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onPickup = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onValve = new UnityEvent<InputAction.CallbackContext>();

        
        
        
        
        
        public CharacterState CharacterState => _characterState == null
            ? (_characterState = GetComponent<CharacterState>())
            : _characterState;


        private PlayerInput PlayerInput => _playerInput;
        
        
        
        public bool JumpPressed
        {
            get => CharacterState.JumpPressed;
            set => CharacterState.JumpPressed = value;
        }

        public bool JumpHeld
        {
            get => CharacterState.JumpHeld;
            set => CharacterState.JumpHeld = value;
        }

        public float MoveX
        {
            get => CharacterState.MoveX;
            set => CharacterState.MoveX = value;
        }

        public float MoveY
        {
            get => CharacterState.MoveY;
            set => CharacterState.MoveY = value;
        }


        public Vector2 MoveInput
        {
            get => CharacterState.MoveInput;
            set => CharacterState.MoveInput = value;
        }


        public bool DropHeldItem { get; set; }


        public Vector2 AimInput { get; set; }

        public bool DropPressed
        {
            get => CharacterState.DropPressed;
            set => CharacterState.DropPressed = value;
        }

        public bool IsAssigned() => PlayerInput != null;

        

        private void Awake()
        {
            _characterState = GetComponent<CharacterState>();
            if (onJump == null) onJump = new UnityEvent<InputAction.CallbackContext>();
            if (onInteract == null) onInteract = new UnityEvent<InputAction.CallbackContext>();
            if (onPickup == null) onPickup = new UnityEvent<InputAction.CallbackContext>();
            onJump.AddListener(OnJump);
            onInteract.AddListener(OnInteract);
            onPickup.AddListener(OnPickup);
        }

        public void AssignPlayer(PlayerInput playerInput)
        {
            //return;
            Debug.Assert(playerInput!=null, "assigned null player input");
            this._playerInput = playerInput;
            enabled = true;
            foreach (var characterInput in GetComponentsInChildren<ICharacterInput>())
            {
                characterInput.AssignPlayer(playerInput);
            }
        }
        public void UnAssignPlayer()
        {
            if(this == null)return;
            this._playerInput = null;
            enabled = false;
            foreach (var characterInput in GetComponentsInChildren<ICharacterInput>())
            {
                characterInput.UnAssignPlayer();
            }
        }

        private void Update()
        {
            // //if (_playerInput == null && player.CharacterInput != null)
            // //{
            // //    _playerInput = player.CharacterInput.PlayerInput;
            // //}
            // debugVariables.hasPlayerAssigned = PlayerInput != null;
            // if (PlayerInput == null)
            //     return;
            //
            //
            // MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
            // AimInput = PlayerInput.actions["Aim"].ReadValue<Vector2>();
            //
            //
            //
            // JumpPressed = PlayerInput.actions["Jump"].WasPressedThisFrame();
            // JumpHeld = PlayerInput.actions["Jump"].IsPressed();
            debugVariables.Update(_characterState);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            MessageBroker.Default.Publish(new JumpActionEvent()
            {
                tag = this.gameObject.tag,
                context = context,
                transform = transform
            });
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            MessageBroker.Default.Publish(new InteractActionEvent()
            {
                tag = this.gameObject.tag,
                context = context,
                transform = transform
            });
        }

        public void OnPickup(InputAction.CallbackContext context)
        {
            MessageBroker.Default.Publish(new PickupActionEvent()
            {
                tag = this.gameObject.tag,
                context = context,
                transform = transform
            });
        }

        
        public void SetHeldItem(Rigidbody2D heldObject)
        {
            CharacterState.heldObject.Value = heldObject;
        }
    }
}