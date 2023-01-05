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

    public enum PlayerAbilityID
    {
        PRIMARY,
        SECONDARY
    }
    
    /// <summary>
    /// called when primary or secondary input is triggered
    /// </summary>
    public struct UseAbilityActionEvent
    {
        public string tag;
        public PlayerAbilityID abilityID;
        public Transform transform;
        public CharacterState characterState;
        public CharacterInputState inputState;
        public InputAction.CallbackContext context;
    }

    /// <summary>
    /// called when select next/prev ability input is triggered
    /// </summary>
    public struct SelectAbilityActionEvent
    {
        public string tag;
        public int abilityIndex;
        public bool isRelative;
        public Transform transform;
        public CharacterState characterState;
        public CharacterInputState inputState;
        public InputAction.CallbackContext context;
    }
    
}