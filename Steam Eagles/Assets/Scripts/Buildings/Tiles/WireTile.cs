using Buildings.Rooms;
using UnityEngine;

namespace Buildings.Tiles
{
    public class WireTile : EditableTile
    {
        public override bool CanTileBePlacedInRoom(Room room)
        {
            if (room == null)
                return false;
            return true;
        }
        public override bool CanTileBeDisconnected()
        {
            return true;
        }

        public override BuildingLayers GetLayer() => BuildingLayers.WIRES;

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            throw new System.NotImplementedException();
        }
    }
}