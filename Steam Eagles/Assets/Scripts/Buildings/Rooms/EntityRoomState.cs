using System;
using CoreLib;
using CoreLib.Entities;
using CoreLib.EntityTag;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    /// <summary>
    /// useful when we wish to know what room a certain entity is in
    /// <para>if we wish to know which entities are in a certain room, use </para>
    /// </summary>
    [RequireComponent(typeof(EntityInitializer))]
    public class EntityRoomState : MonoBehaviour
    {
        private DynamicReactiveProperty<Room> _currentRoom = new DynamicReactiveProperty<Room>();
        
        public IReadOnlyReactiveProperty<Room> CurrentRoom => _currentRoom;
        
        
        [ShowInInspector, ReadOnly]
        public string CurrentRoomName { get; private set; }
        
        public void SetCurrentRoom(Room room)
        {
            _currentRoom.Value = room;
            CurrentRoomName = room == null ? "Not In Room" : room.name;
        }

        private void Awake()
        {
            _currentRoom.OnSwitched.Subscribe(t =>
            {
                if(t.previous!=null) OnLeftRoom(t.previous);
                if(t.next!=null) OnEnteredRoom(t.next);
            });
        }

        private void OnEnteredRoom(Room objNext)
        {
            var entities = objNext.GetOrAddComponent<RoomEntities>();
            entities.AddEntity(gameObject);
        }

        private void OnLeftRoom(Room objPrevious)
        {
            var entities = objPrevious.GetOrAddComponent<RoomEntities>();
            entities.RemoveEntity(gameObject);
        }
    }
}