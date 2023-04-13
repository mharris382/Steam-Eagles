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

        public float aimSpeed = 10f;
        public Vector3 aimOriginOffset;
        public float aimSmoothing = 0.1f;
        
        
        
        private CharacterInputState _inputState;
        private ToolState _toolState;
        private float moveY;

        private void Awake()
        {
            _inputState = GetComponent<CharacterInputState>();
            _toolState = GetComponent<ToolState>();
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

            
            
            _toolState.Inputs.AimInputRaw = aimInput;
            _toolState.Inputs.UsePressed = inputPlayer.actions["Ability Primary"].WasPressedThisFrame();
            _toolState.Inputs.CancelPressed = inputPlayer.actions["Ability Secondary"].WasPressedThisFrame() || 
                                              inputPlayer.actions["Inventory"].WasPressedThisFrame() || 
                                              inputPlayer.actions["Map"].WasPressedThisFrame() ||
                                              inputPlayer.actions["Pause"].WasPressedThisFrame() ||
                                              inputPlayer.actions["Codex"].WasPressedThisFrame() ||
                                              inputPlayer.actions["Characters"].WasPressedThisFrame();
            if (_toolState.Inputs.UsePressed) LogInput("UsePressed");
            if (_toolState.Inputs.CancelPressed) LogInput("CancelPressed");

            bool usingMouse = inputPlayer.currentControlScheme.Contains("Keyboard");
            HandleToolAim(Time.deltaTime, false);
        }

        private Vector2 _aimVelocity;

        void LogInput(string input)
        {
            Debug.Log($"INPUT: {input}",this);
        }

        void HandleToolAim(float dt, bool usingMouse)
        {
            var pos = (Vector2) _toolState.AimPositionLocal;
            var aimOrigin =(Vector2) aimOriginOffset;
            if (!usingMouse)
            {
                var aimInput = _toolState.Inputs.AimInputRaw * (aimSpeed * dt);
                pos += aimInput;
            }
            else
            {
                
                pos =  _toolState.transform.InverseTransformVector(player.playerCamera.Value.ScreenToWorldPoint(Input.mousePosition));
                _toolState.AimPositionWorld = pos;
                return;
            }
            
            var diff = pos - aimOrigin;
            if (diff.sqrMagnitude > _toolState.SqrMaxToolRange)
            {
                pos = aimOrigin + diff.normalized * _toolState.maxToolRange;
            }
            _toolState.AimPositionLocal = pos;
        }
    }
}
