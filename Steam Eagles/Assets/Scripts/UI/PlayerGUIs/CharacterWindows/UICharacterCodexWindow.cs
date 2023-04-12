namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterCodexWindow : UICharacterWindowBase
    {
        private const string CODEX = "Codex";
        
        public override string GetActionName() => CODEX;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.CODEX;
    }
}