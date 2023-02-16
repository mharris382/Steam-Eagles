using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Players;
using Sirenix.OdinInspector;
using UnityEngine;

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
        return player.CharacterInput != null && player.CharacterInput.PlayerInput != null;
    }
    private void Update()
    {
        if (!HasInput())
        {
            return;
        }

        var inputPlayer = player.CharacterInput.PlayerInput;
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