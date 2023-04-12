using System;
using Buildings.Rooms;
using UnityEngine;

namespace Buildings
{
    /// <summary>
    /// implemented by BuildingMap
    /// </summary>
    public interface IBuildingRoomLookup
    {
        public Room GetRoom(Vector3Int cell, BuildingLayers layers);

        public BoundsInt GetCellsForRoom(Room room, BuildingLayers layers);
    }
}