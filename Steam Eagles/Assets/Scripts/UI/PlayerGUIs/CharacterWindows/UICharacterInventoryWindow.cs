using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterInventoryWindow : UICharacterWindowBase
    {
        private const string INVENTORY = "Inventory";

        public override string GetActionName() => INVENTORY;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.INVENTORY;

        protected override void InitializeWindow(GameObject character, PlayerInput playerInput)
        {
            Debug.Log($"Opening Inventory for {character.name} with {playerInput.name}");
            base.InitializeWindow(character, playerInput);
            playerInput.SwitchCurrentActionMap("UI");
        }
    }
}