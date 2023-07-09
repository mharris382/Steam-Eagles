using UnityEngine;
using UnityEngine.InputSystem;


public class CraftingDirectionHandler
{
    public bool IsFlipped
    {
        get; set;
    }
    public CraftingDirectionHandler() { }

    public void UpdateDirection(PlayerInput playerInput)
    {
        var movement = playerInput.actions["Move"].ReadValue<Vector2>();
        if (Mathf.Abs(movement.x) != 0)
        {
            IsFlipped = movement.x < 0;
        }
    }
}