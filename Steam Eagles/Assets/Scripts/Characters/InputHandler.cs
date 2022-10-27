using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterState))]
public class InputHandler : MonoBehaviour
{
    private CharacterState _state;
    [SerializeField]
    private string horizontalAxisName = "Horizontal";

    [SerializeField] private string verticalAxisName = "Vertical";

    [SerializeField] private string jumpButtonName = "Jump";

    [SerializeField] private bool useAdditionalJumpInput;
    [SerializeField] private KeyCode additionalJumpInput = KeyCode.Space;
    private void Awake()
    {
        _state = GetComponent<CharacterState>();
    }

    public bool useLegacyInput;
    public void OnMove(InputAction.CallbackContext context)
    {
        if (useLegacyInput) return;
        var moveInput = context.ReadValue<Vector2>();
        _state.MoveX = moveInput.x;
        _state.MoveY = moveInput.y;
    }
    public void OnMoveHorizontal(InputAction.CallbackContext context)
    {
        if (useLegacyInput) return;
        _state.MoveX = context.ReadValue<float>();
    }
    
    public void OnMoveVertical(InputAction.CallbackContext context)
    {
        if (useLegacyInput) return;
        _state.MoveY = context.ReadValue<float>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (useLegacyInput) return;
        _state.JumpPressed = context.ReadValueAsButton();
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }
    
    public void OnPreview(InputAction.CallbackContext context)
    {
        
    }
    
    public void OnAim(InputAction.CallbackContext context)
    {
        
    }
    
    public void OnQueueForDisconnect(InputAction.CallbackContext context)
    {
        
    }
    
    public void OnQueueForBuild(InputAction.CallbackContext context)
    {
        
    }

    public void OnExecuteNearby(InputAction.CallbackContext context)
    {
        
    }


    private PlayerInput _playerInput;
    private void Update()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (useLegacyInput)
        {
            _state.JumpPressed = IsJumpPressed();
            _state.JumpHeld = IsJumpHeld();
            _state.MoveX = Input.GetAxis(horizontalAxisName);
            _state.MoveY = Input.GetAxis(verticalAxisName);
        }
        else
        {
            _state.JumpPressed = _playerInput.actions["Jump"].WasPressedThisFrame();
            _state.JumpHeld = _playerInput.actions["Jump"].IsPressed();
        }
        
    }

    private bool IsJumpHeld()
    {
        if (useAdditionalJumpInput && Input.GetKey(additionalJumpInput))
            return true;
        return Input.GetButton(jumpButtonName);
    }

    private bool IsJumpPressed()
    {
        if (useAdditionalJumpInput && Input.GetKeyDown(additionalJumpInput))
            return true;
        return Input.GetButtonDown(jumpButtonName);
    }
}

[RequireComponent(typeof(CharacterState))]
public class NewInputHandler : MonoBehaviour
{
    
}