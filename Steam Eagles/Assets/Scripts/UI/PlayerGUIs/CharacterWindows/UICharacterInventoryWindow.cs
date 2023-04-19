using System;
using System.Linq;
using Items;
using Items.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterInventoryWindow : UICharacterWindowBase
    {
        private const string INVENTORY = "Inventory";
        
        public UIItemCollection itemCollection;

        public override string GetActionName() => INVENTORY;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.INVENTORY;

        protected override void InitializeWindow(GameObject character, PlayerInput playerInput)
        {
            Debug.Log($"Opening Inventory for {character.name} with {playerInput.name}");
            base.InitializeWindow(character, playerInput);
            var inventories = character.GetComponentsInChildren<Inventory>();
            var mainInventory = inventories.FirstOrDefault(t => t.isMain);
            var toolBeltInventory = inventories.FirstOrDefault(t => t.isToolbelt);
            Debug.Assert(mainInventory != null, "mainInventory == null", mainInventory);
            Debug.Assert(toolBeltInventory != null, "toolBeltInventory == null", toolBeltInventory);
            InitializeWindows(mainInventory, toolBeltInventory);
        }

        private void InitializeWindows(Inventory mainInventory, Inventory toolBeltInventory)
        {
            itemCollection.PopulateContainer(mainInventory.Items.ToList());
        }
    }
}