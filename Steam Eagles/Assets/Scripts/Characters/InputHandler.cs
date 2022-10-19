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
    
    
    private void Awake()
    {
        _state = GetComponent<CharacterState>();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        var moveInput = context.ReadValue<Vector2>();
        _state.MoveX = moveInput.x;
        _state.MoveY = moveInput.y;
    }
    public void OnMoveHorizontal(InputAction.CallbackContext context)
    {
        _state.MoveX = context.ReadValue<float>();
    }
    
    public void OnMoveVertical(InputAction.CallbackContext context)
    {
        _state.MoveY = context.ReadValue<float>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        
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

        _state.JumpPressed = _playerInput.actions["Jump"].WasPressedThisFrame();
        _state.JumpHeld = _playerInput.actions["Jump"].IsPressed();
        
    }
}

[RequireComponent(typeof(CharacterState))]
public class NewInputHandler : MonoBehaviour
{
    
}