using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    public class EntityRoomState : MonoBehaviour
    {
        private ReactiveProperty<Room> _currentRoom = new ReactiveProperty<Room>();
        
        public IReadOnlyReactiveProperty<Room> CurrentRoom => _currentRoom;
        
        
        [ShowInInspector, ReadOnly]
        public string CurrentRoomName { get; private set; }
        
        public void SetCurrentRoom(Room room)
        {
            _currentRoom.Value = room;
            CurrentRoomName = room == null ? "Not In Room" : room.name;
        }
    }
}