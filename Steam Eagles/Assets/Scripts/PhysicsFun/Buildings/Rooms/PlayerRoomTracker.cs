using Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    /// <summary>
    /// there will be one instance of this class for each player
    /// </summary>
    public class PlayerRoomTracker : MonoBehaviour
    {
        [Required] 
        public Player player;
        
        
    }
}