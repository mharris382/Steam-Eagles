using Players;
using Sirenix.OdinInspector;
using UnityEngine;


//TODO: this should be refactored so that character input processor is not attached to a character as a component and is instead passed a player instance and a character instance
namespace Characters.MyInput
{
    /// <summary>
    /// actually processes the input from the player and sets the input state on the character
    /// </summary>
    public class CharacterInputProcessor : MonoBehaviour
    {
        [Required]
        public Player player;

        private CharacterInputState _inputState;
        private float moveY;

        private void Awake()
        {
            _inputState = GetComponent<CharacterInputState>();
        }

        bool HasInput()
        {
            return player.InputWrapper != null && player.InputWrapper.PlayerInput != null;
        }
        private void Update()
        {
            if (!HasInput())
            {
                return;
            }
            var inputPlayer = player.InputWrapper.PlayerInput;
            var moveInput = inputPlayer.actions["Move"].ReadValue<Vector2>();
        
        
        
            var aimInput = inputPlayer.actions["Aim"].ReadValue<Vector2>();
            var jumpAction = inputPlayer.actions["Jump"];
        
            var moveX = moveInput.x;
            this.moveY = moveInput.y;
            bool jumpPressed = jumpAction.WasPressedThisFrame();
            bool jumpHeld = jumpAction.IsPressed();
        
            if (moveY < -0.5f)
            {
                _inputState.DropPressed = jumpHeld;
            }
            else
            {
           
                _inputState.JumpPressed = jumpPressed;
                _inputState.JumpHeld = jumpHeld;
            }
        
        

            _inputState.MoveInput = moveInput;
            _inputState.AimInput = aimInput;
        }
    }
}
