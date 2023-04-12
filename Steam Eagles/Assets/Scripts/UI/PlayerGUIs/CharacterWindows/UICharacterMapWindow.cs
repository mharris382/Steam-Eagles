namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterMapWindow : UICharacterWindowBase
    {
        private const string MAP = "Map";

        public override string GetActionName() => MAP;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.MAP;
    }
}