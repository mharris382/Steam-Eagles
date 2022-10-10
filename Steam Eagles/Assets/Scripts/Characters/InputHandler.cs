using UnityEngine;

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
    
    
    private void Update()
    {
        _state.JumpPressed = Input.GetButtonDown(jumpButtonName);
        _state.JumpHeld = Input.GetButton(jumpButtonName);
        
        _state.MoveX = Input.GetAxis(horizontalAxisName);
        _state.MoveY = Input.GetAxisRaw(verticalAxisName);
        
    }
}