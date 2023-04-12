using Buildings.Rooms;
using UnityEngine;

namespace Buildings
{
    public interface IBuildingRoomLookup
    {
        public Room GetRoom(Vector3Int cell, BuildingLayers layers);

        public BoundsInt GetCellsForRoom(Room room, BuildingLayers layers);
    }
}