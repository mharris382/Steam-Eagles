using System;
using System.Collections;
using Characters;
using Characters.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
[Obsolete("using new input system")]
public class AbilityInputHandler : MonoBehaviour
{

    public AbilityController leftAbility;
    public AbilityController rightAbility;
    public AbilityController upAbility;
    public AbilityController downAbility;
    
    [Header("Legacy Keyboard Input")]
    public bool useLegacyInput = false;
    
    public KeyCode keyUp = KeyCode.Alpha2;
    public KeyCode keyDown = KeyCode.X;
    public KeyCode keyRight = KeyCode.E;
    public KeyCode keyLeft = KeyCode.Q;
    
    [Header("Legacy Mouse Input")]
    public bool useMouseInput = false;

    private MouseAbilityInputHandler mouseInput;

    private void Awake()
    {
        this.mouseInput = GetComponent<MouseAbilityInputHandler>();
        this.mouseInput.enabled = useMouseInput;
    }

    public void OnActionLeft(InputAction.CallbackContext context)
    {
       // leftAbility.TryAbility();
    }
    
    public void OnActionRight(InputAction.CallbackContext context)
    {
       // rightAbility.TryAbility();
    }
    
    public void OnActionUp(InputAction.CallbackContext context)
    {
       // upAbility.TryAbility();
    }
    
    public void OnActionDown(InputAction.CallbackContext context)
    {
      //  downAbility.TryAbility();
    }

    private void Update()
    {
        if (useLegacyInput) 
            LegacyInputKeyboard();
        
        this.mouseInput.enabled = useMouseInput;
    }

    private void LegacyInputKeyboard()
    {
        if (Input.GetKeyDown(keyUp))
        {
         //   upAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyDown))
        {
         //   downAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyLeft))
        {
        //    leftAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyRight))
        {
        //    rightAbility.TryAbility();
        }
    }
}