using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [Obsolete("Use CharacterInputProcessorV3")]
    public class CharacterInputProcessorV2  : IDisposable
    {
        private readonly Character _character;
        private readonly ToolState toolState;
        private readonly ReadOnlyReactiveProperty<PlayerInput> _assignedPlayerInput;
        private readonly bool _debug;
        private readonly IDisposable _disposable;

        public CharacterInputProcessorV2(Character character,
            ReadOnlyReactiveProperty<PlayerInput> assignedPlayerInput,
            bool debug)
        {
            this._character = character;
            toolState = this._character.Tool;
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
                _character.DropPressed = jumpHeld;
                _character.JumpPressed = _character.JumpHeld = false;
            }
            else
            {
                _character.DropPressed = false;
                _character.JumpPressed = jumpPressed;
                _character.JumpHeld = jumpHeld;
            }

            _character.MoveInput = moveInput;
            toolState.Inputs.AimInputRaw = aimInput;
            toolState.Inputs.UsePressed = playerInput.actions["Use"].WasPressedThisFrame();
            toolState.Inputs.CancelPressed = playerInput.actions["Cancel"].WasPressedThisFrame();

        }
        
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}