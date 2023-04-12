namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterInventoryWindow : UICharacterWindowBase
    {
        private const string INVENTORY = "Inventory";

        public override string GetActionName() => INVENTORY;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.INVENTORY;
    }
}