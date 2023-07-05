using System;
using UniRx;
using UnityEngine.InputSystem;

[Serializable]
public class UICraftingMode
{
    public string destructModeAction = "Crafting Destruct Mode";

    public BoolReactiveProperty isDestructMode = new();
    public void Process(PlayerInput playerInput)
    {
        isDestructMode.Value = playerInput.actions[destructModeAction].IsPressed();
    }
}