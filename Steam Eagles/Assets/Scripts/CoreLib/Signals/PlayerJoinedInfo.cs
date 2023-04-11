using System;

namespace CoreLib
{
    /// <summary>
    /// event raised when a player is assigned a controller scheme (may not yet
    /// be assigned to a character)
    /// </summary>
    [Obsolete("Use PlayerDeviceJoined instead")]
    public struct PlayerJoinedInfo
    {
        public int playerNumber;
        public UnityEngine.Object playerObject;
        
        public PlayerJoinedInfo(int playerNumber, UnityEngine.Object playerObject)
        {
            this.playerNumber = playerNumber;
            this.playerObject = playerObject;
        }
    }
}