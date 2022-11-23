using System;
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


        

        private void Awake()
        {
            if (onJump == null) onJump = new UnityEvent<InputAction.CallbackContext>();
            if (onInteract == null) onInteract = new UnityEvent<InputAction.CallbackContext>();
            if (onPickup == null) onPickup = new UnityEvent<InputAction.CallbackContext>();
            onJump.AddListener(OnJump);
            onInteract.AddListener(OnInteract);
            onPickup.AddListener(OnPickup);
        }

        public void AssignPlayer(PlayerInput playerInput)
        {
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
            if (PlayerInput == null) return;

            MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
            AimInput = PlayerInput.actions["Aim"].ReadValue<Vector2>();

            if (useEventsForJump) return;

            JumpPressed = PlayerInput.actions["Jump"].WasPressedThisFrame();
            JumpHeld = PlayerInput.actions["Jump"].IsPressed();
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