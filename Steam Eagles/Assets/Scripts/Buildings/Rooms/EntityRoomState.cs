using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    public class EntityRoomState : MonoBehaviour
    {
        private ReactiveProperty<Room> _currentRoom = new ReactiveProperty<Room>();
        
        public IReadOnlyReactiveProperty<Room> CurrentRoom => _currentRoom;
        
        public void SetCurrentRoom(Room room)
        {
            _currentRoom.Value = room;
        }
    }
}