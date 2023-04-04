using UnityEngine;

namespace CoreLib
{
    
    /// <summary>
    /// event raised when a player is assigned to a character
    /// </summary>
    public struct PlayerCharacterBoundInfo
    {
        public int playerNumber;
        public string character;
    }
    
    public struct PlayerCharacterUnboundInfo
    {
        public int playerNumber;
    }
}