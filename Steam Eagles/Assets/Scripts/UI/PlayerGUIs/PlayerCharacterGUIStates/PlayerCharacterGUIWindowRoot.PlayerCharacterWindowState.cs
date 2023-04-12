using FSM;
using UI.PlayerGUIs.CharacterWindows;

namespace UI.PlayerGUIs
{
    public partial class PlayerCharacterGUIWindowRoot
    {
        public class PlayerCharacterWindowState : PlayerGUIState
        {
            private readonly UICharacterWindowController _windowController;

            private StateMachine _stateMachine;
            
            public PlayerCharacterWindowState(PlayerCharacterGUIWindowRoot guiController, 
                PlayerCharacterHUD playerCharacterHUD, UICharacterWindowController windowController) : base(guiController, playerCharacterHUD)
            {
                _windowController = windowController;
                _stateMachine = _windowController.CreateWindowStateMachine();
            }

            public override void OnEnter()
            {
                base.OnEnter();
                GUIController.Open();
                PlayerCharacterHUD.Close();
            }

            public override void OnLogic()
            {
                base.OnLogic();
                _stateMachine.OnLogic();
            }

            public override void OnExit()
            {
                base.OnExit();
                GUIController.Close();
                PlayerCharacterHUD.Open();
            }
        }
    }
}