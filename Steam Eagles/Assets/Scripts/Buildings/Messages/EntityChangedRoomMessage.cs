using Buildings.Rooms;
using CoreLib.Entities;

namespace Buildings.Messages
{
    public struct EntityChangedRoomMessage
    {
        public Entity Entity { get; }
        public Room Room { get; }

        public EntityChangedRoomMessage(Entity entity, Room room)
        {
            Entity = entity;
            Room = room;
        }
    }
}