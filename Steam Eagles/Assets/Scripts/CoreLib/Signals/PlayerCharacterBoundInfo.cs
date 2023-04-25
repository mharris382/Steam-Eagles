using UnityEngine;

namespace CoreLib
{
    
    /// <summary>
    /// event raised when a player is assigned to a character, triggered from UI. NOTE THE CHARACTER MAY NOT ACTUALLY BE SPAWNED YET.
    /// If you want to know when a player is assigned to a character that has been spawned <see cref="CharacterAssignedPlayerInputInfo"/>
    /// </summary>
    public struct PlayerCharacterBoundInfo
    {
        public int playerNumber;
        public string character;
    }

    public struct CharacterAssignedPlayerInputInfo
    {
        public string characterName;
        public int playerNumber;
        public GameObject inputGo;
        public Component characterState;
        public GameObject camera;
    }
    
    public struct CharacterUnassignedPlayerInputInfo
    {
        public string characterName;
        public GameObject inputGo;
        public Component characterState;
    }
    
    public struct PlayerCharacterUnboundInfo
    {
        public int playerNumber;
    }
}