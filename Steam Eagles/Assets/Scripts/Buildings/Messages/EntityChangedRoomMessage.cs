using Buildings.Rooms;
using CoreLib.MyEntities;

namespace Buildings.Messages
{
    public struct EntityChangedRoomMessage
    {
        public EntityInitializer Entity { get; }
        public Room Room { get; }

        public EntityChangedRoomMessage(EntityInitializer entity, Room room)
        {
            Entity = entity;
            Room = room;
        }
    }
}