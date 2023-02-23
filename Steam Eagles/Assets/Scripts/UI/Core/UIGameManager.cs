using System;
using CoreLib;
using FSM;
using Players;

namespace UI.Core
{
    public class UIGameManager : Singleton<UIGameManager>
    {
         private FSM.StateMachine _rootStateMachine;
         

        private void Awake()
        {
            _rootStateMachine = new FSM.StateMachine();
        }
    }

    public class UIPauseMenuState : UIState
    {
        private readonly Player playerWhoPaused;

        public UIPauseMenuState(Player playerWhoPaused) : base()
        {
            
            this.playerWhoPaused = playerWhoPaused;
        }
    }
}