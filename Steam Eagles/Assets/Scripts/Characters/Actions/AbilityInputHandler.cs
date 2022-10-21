using System;
using System.Collections;
using Characters.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityInputHandler : MonoBehaviour
{

    public Ability leftAbility;
    public Ability rightAbility;
    public Ability upAbility;
    public Ability downAbility;
    public bool useLegacyInput = false;
    public KeyCode keyUp = KeyCode.Alpha2;
    public KeyCode keyDown = KeyCode.X;
    public KeyCode keyRight = KeyCode.E;
    public KeyCode keyLeft = KeyCode.Q;
    
    public void OnActionLeft(InputAction.CallbackContext context)
    {
        leftAbility.TryAbility();
    }
    
    public void OnActionRight(InputAction.CallbackContext context)
    {
        rightAbility.TryAbility();
    }
    
    public void OnActionUp(InputAction.CallbackContext context)
    {
        upAbility.TryAbility();
    }
    
    public void OnActionDown(InputAction.CallbackContext context)
    {
        downAbility.TryAbility();
    }

    private void Update()
    {
        if (!useLegacyInput) return;

        if (Input.GetKeyDown(keyUp))
        {
            upAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyDown))
        {
            downAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyLeft))
        {
            leftAbility.TryAbility();
        }
        else if (Input.GetKeyDown(keyRight))
        {
            rightAbility.TryAbility();
        }
}
}