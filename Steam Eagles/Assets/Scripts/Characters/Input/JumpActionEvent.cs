using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    /// <summary>
    /// called when jump input is pressed
    /// </summary>
    public struct JumpActionEvent
    {
        public string tag;
        public Transform transform;
        public CharacterState characterState;
        public InputAction.CallbackContext context;
    }
    
    /// <summary>
    /// called when interact input is pressed
    /// </summary>
    public struct InteractActionEvent
    {
        public string tag;
        public Transform transform;
        public CharacterState characterState;
        public InputAction.CallbackContext context;
    }
    
    /// <summary>
    /// called when pickup input is pressed
    /// </summary>
    public struct PickupActionEvent
    {
        public string tag;
        public Transform transform;
        public CharacterState characterState;
        public InputAction.CallbackContext context;
    }
    
    /// <summary>
    /// called when pickup input is pressed
    /// </summary>
    public struct ValveActionEvent
    {
        public string tag;
        public Transform transform;
        public CharacterState characterState;
        public InputAction.CallbackContext context;
    }

    
}