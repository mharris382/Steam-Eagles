using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [Obsolete("Use CharacterInputProcessorV3")]
    public class CharacterInputProcessorV2  : IDisposable
    {
        private readonly CharacterState characterState;
        private readonly ToolState toolState;
        private readonly ReadOnlyReactiveProperty<PlayerInput> _assignedPlayerInput;
        private readonly bool _debug;
        private readonly IDisposable _disposable;

        public CharacterInputProcessorV2(CharacterState characterState,
            ReadOnlyReactiveProperty<PlayerInput> assignedPlayerInput,
            bool debug)
        {
            this.characterState = characterState;
            toolState = this.characterState.Tool;
            _assignedPlayerInput = assignedPlayerInput;
            _debug = debug;
            CompositeDisposable cd = new CompositeDisposable();
            
            assignedPlayerInput
                .Select(t => t != null ? Observable.EveryUpdate() : Observable.Empty<long>())
                .Switch()
                .Subscribe(OnUpdateAssigned)
                .AddTo(cd);
            
            _disposable = cd;
        }

        void OnUpdateAssigned(long _)
        {
            if (_debug)
                Debug.Log("On Update Assigned Called");
            
            Debug.Assert(_assignedPlayerInput.Value != null, "_assignedPlayerInput.Value is null");
            if(_assignedPlayerInput.Value == null)
                return;
            
            ProcessInput();
        }

        public void ProcessInput()
        {
            PlayerInput playerInput = _assignedPlayerInput.Value;
            if (playerInput == null)
                return;

            bool usingKeyboardMouseInput = playerInput.currentControlScheme.Contains("Keyboard");
            
            var moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            var aimInput = playerInput.actions["Aim"].ReadValue<Vector2>();
            var jumpAction = playerInput.actions["Jump"];

            bool jumpPressed = jumpAction.WasPressedThisFrame();
            bool jumpHeld = jumpAction.IsPressed();
            if (moveInput.y < -0.5f)
            {
                characterState.DropPressed = jumpHeld;
                characterState.JumpPressed = characterState.JumpHeld = false;
            }
            else
            {
                characterState.DropPressed = false;
                characterState.JumpPressed = jumpPressed;
                characterState.JumpHeld = jumpHeld;
            }

            characterState.MoveInput = moveInput;
            toolState.Inputs.AimInput = aimInput;
            
            
        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}