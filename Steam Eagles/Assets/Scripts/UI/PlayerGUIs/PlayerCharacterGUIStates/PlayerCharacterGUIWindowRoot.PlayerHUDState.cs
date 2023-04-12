namespace UI.PlayerGUIs
{
    public partial class PlayerCharacterGUIWindowRoot
    {
        public class PlayerHUDState : PlayerGUIState
        {
            public PlayerHUDState(PlayerCharacterGUIWindowRoot guiController, PlayerCharacterHUD playerCharacterHUD) : base(guiController, playerCharacterHUD){ }

            public override void OnEnter()
            {
                GUIController.Close();
                PlayerCharacterHUD.Open();
            }
        }
    }
}