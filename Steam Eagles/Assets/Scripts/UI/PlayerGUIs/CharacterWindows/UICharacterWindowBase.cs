using UnityEngine;

namespace UI.PlayerGUIs.CharacterWindows
{
    public abstract class UICharacterWindowBase : Window
    {
        public abstract string GetActionName();
        
        public abstract UICharacterWindowController.CharacterWindowState GetWindowState();
    }
}