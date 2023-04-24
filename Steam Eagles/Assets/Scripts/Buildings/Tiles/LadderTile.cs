﻿using Buildings.Rooms;
using UnityEngine;

namespace Buildings.Tiles
{
    [CreateAssetMenu(menuName = "Steam Eagles/Tiles/Ladder Tile")]
    public class LadderTile : EditableTile
    {
        public override bool CanTileBePlacedInRoom(Room room)
        {
            return room.buildLevel != BuildLevel.NONE;
        }
        public override bool CanTileBeDisconnected()
        {
            return false;
        }

        public override BuildingLayers GetLayer() => BuildingLayers.LADDERS;

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            throw new System.NotImplementedException();
        }
    }
}