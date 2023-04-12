namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterCharactersWindow : UICharacterWindowBase
    {
        private const string CHARACTERS = "Characters";
        
        public override string GetActionName() => CHARACTERS;
        public override UICharacterWindowController.CharacterWindowState GetWindowState() => UICharacterWindowController.CharacterWindowState.CHARACTERS;
    }
}