using Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Rooms
{
    [System.Obsolete("use CharacterRoomTracker instead")]
    /// <summary>
    /// there will be one instance of this class for each player
    /// </summary>
    public class PlayerRoomTracker : MonoBehaviour
    {
        [Required] 
        public Player player;
        
        
    }
}