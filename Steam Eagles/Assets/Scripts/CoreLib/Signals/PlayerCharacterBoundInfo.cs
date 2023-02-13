using UnityEngine;

namespace CoreLib
{
    
    /// <summary>
    /// event raised when a player is assigned to a character
    /// </summary>
    public struct PlayerCharacterBoundInfo
    {
        public int playerNumber;
        public GameObject character;
    }
    
    public struct PlayerCharacterUnboundInfo
    {
        
    }
}