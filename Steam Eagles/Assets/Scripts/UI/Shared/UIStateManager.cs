using System;
using CoreLib;
using UnityEngine;

namespace UI.Shared
{
    public class UIStateManager : Singleton<UIStateManager>
    {
        [SerializeField]
        private bool hasGameStarted;
        
        [SerializeField]
        private GameMode gameMode;
        
        public bool HasGameStarted
        {
            get => hasGameStarted;
            private set => hasGameStarted = value;
        }


        public GameMode GameMode
        {
            get => gameMode;
            private set => gameMode = value;
        }

        public void StartGame(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.SETUP:
                    
                    break;
                case GameMode.SINGLEPLAYER:
                    StartGameInSinglePlayerMode();
                    break;
                case GameMode.LOCAL_MULTIPLAYER:
                    StartGameInMultiplayerPlayerMode();
                    break;
                case GameMode.ONLINE_MULTIPLAYER:
                    StartGameInOnlineMultiplayerMode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null);
            }
        }

        private bool AbleToStartGameInSingleplayerMode()
        {

            return false;
        }
        
        private void StartGameInSinglePlayerMode()
        {
            if (AbleToStartGameInSingleplayerMode())
            {
                gameMode = GameMode.SINGLEPLAYER;
            }
        }
        
        
        private bool AbleToStartGameInLocalMultiplayerMode()
        {
            return false;
        }
        private void StartGameInMultiplayerPlayerMode()
        {
            if (AbleToStartGameInLocalMultiplayerMode())
            {
                gameMode = GameMode.LOCAL_MULTIPLAYER;
            }
        }
        
        private bool AbleToStartGameInOnlineMultiplayerMode()
        {
            return false;
        }
        private void StartGameInOnlineMultiplayerMode()
        {
            if (AbleToStartGameInOnlineMultiplayerMode())
            {
                gameMode = GameMode.ONLINE_MULTIPLAYER;
            }
        }
        
    }

    public enum GameMode
    {
        SETUP,
        SINGLEPLAYER,
        LOCAL_MULTIPLAYER,
        ONLINE_MULTIPLAYER
    }
}