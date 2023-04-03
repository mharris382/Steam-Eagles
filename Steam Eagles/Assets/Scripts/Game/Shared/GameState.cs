using UnityEngine;

namespace Game.Shared
{
    public class GameState
    {
        public bool IsGameRunning { get; set; }
        
        public bool IsGamePaused { get; set; }
        
        
        
        public class Player
        {
            public int playerIndex;
            public GameObject playerInputGameObject;
            public string assignedCharacterName;
        }
    }
}