using FSM;

namespace UI.PlayerGUIs
{
    public partial class PlayerCharacterGUIWindowRoot
    {
        public abstract class PlayerGUIState : StateBase
        {
            public PlayerCharacterGUIWindowRoot GUIController { get; }
            public PlayerCharacterHUD PlayerCharacterHUD { get; }

            public PlayerGUIState(PlayerCharacterGUIWindowRoot guiController, PlayerCharacterHUD playerCharacterHUD) : base(false)
            {
                GUIController = guiController;
                PlayerCharacterHUD = playerCharacterHUD;
            }
        }
    }
}