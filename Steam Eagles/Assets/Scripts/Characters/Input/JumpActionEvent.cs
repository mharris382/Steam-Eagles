using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public struct JumpActionEvent
    {
        public string tag;
        public Transform transform;
        public InputAction.CallbackContext context;
    }
    
    public struct InteractActionEvent
    {
        public string tag;
        public Transform transform;
        public InputAction.CallbackContext context;
    }
    
    public struct PickupActionEvent
    {
        public string tag;
        public Transform transform;
        public InputAction.CallbackContext context;
    }
}