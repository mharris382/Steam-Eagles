using CoreLib.EntityTag;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    [RequireComponent(typeof(Room))]
    public class RoomState : MonoBehaviour
    {
        RoomTextures _roomTextures;
        Room _room;
        RoomEntities _roomEntities;
        
        public Room Room => _room ? _room : _room = GetComponent<Room>();
        public RoomTextures RoomTextures => _roomTextures ? _roomTextures : _roomTextures = Room.GetOrAddComponent<RoomTextures>();
        public RoomEntities RoomEntities => _roomEntities ? _roomEntities : _roomEntities = Room.GetOrAddComponent<RoomEntities>();
    }
}