using System;
using Game;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Characters
{
    /// <summary>
    /// acts as a proxy for passing input to the player character
    /// </summary>
    [RequireComponent(typeof(Character))]
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

            public void Update(Character State)
            {
                xInput = State.MoveX;
                yInput = State.MoveY;
                jumpHeld = State.JumpHeld;
                jumpPressed = State.JumpPressed;
            }
        }
        
        [SerializeField] private DebugVariables debugVariables;
        [SerializeField] private bool useEventsForJump;
        
        private Character _character;
        private PlayerInput _playerInput;


        
        public UnityEvent<InputAction.CallbackContext> onJump = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onInteract = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onPickup = new UnityEvent<InputAction.CallbackContext>();
        public UnityEvent<InputAction.CallbackContext> onValve = new UnityEvent<InputAction.CallbackContext>();

        
        
        
        
        
        public Character Character => _character == null
            ? (_character = GetComponent<Character>())
            : _character;


        private PlayerInput PlayerInput => _playerInput;
        
        
        
        public bool JumpPressed
        {
            get => Character.JumpPressed;
            set => Character.JumpPressed = value;
        }

        public bool JumpHeld
        {
            get => Character.JumpHeld;
            set => Character.JumpHeld = value;
        }

        public float MoveX
        {
            get => Character.MoveX;
            set => Character.MoveX = value;
        }

        public float MoveY
        {
            get => Character.MoveY;
            set => Character.MoveY = value;
        }


        public Vector2 MoveInput
        {
            get => Character.MoveInput;
            set => Character.MoveInput = value;
        }


        public bool DropHeldItem { get; set; }


        public Vector2 AimInput { get; set; }

        public bool DropPressed
        {
            get => Character.DropPressed;
            set => Character.DropPressed = value;
        }

        public bool IsAssigned() => PlayerInput != null;

        

        private void Awake()
        {
            _character = GetComponent<Character>();
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
            debugVariables.Update(_character);
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
            Character.heldObject.Value = heldObject;
        }
    }
}