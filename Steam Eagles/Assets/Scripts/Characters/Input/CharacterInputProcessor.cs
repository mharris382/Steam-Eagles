using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Players;
using Sirenix.OdinInspector;
using UnityEngine;

[Obsolete("Needs to be replaced by a POCO version which is not attached to the character or the input")]
//TODO: this should be refactored so that character input processor is not attached to a character as a component and is instead passed a player instance and a character instance
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
