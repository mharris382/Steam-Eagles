namespace UI.PlayerGUIs
{
    public partial class PlayerCharacterGUIWindowRoot
    {
        public class PlayerGUIDisabledState : PlayerGUIState
        {
            public PlayerGUIDisabledState(PlayerCharacterGUIWindowRoot guiController, PlayerCharacterHUD playerCharacterHUD) : base(guiController, playerCharacterHUD){ }

            public override void OnEnter()
            {
                GUIController.Close();
                PlayerCharacterHUD.Close();
            }
        }
    }
}