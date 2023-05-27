using System.Collections.Generic;
using Buildings.Rooms;

namespace Buildings.CoreData
{
    public class RoomCoreData
    {
        private readonly Room _room;
        private readonly List<Room> _neighbors;

        public RoomCoreData(Rooms.Room room, List<Room> neighbors)
        {
            _room = room;
            _neighbors = neighbors;
        }
    }
}