using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Tiles
{
    [CreateAssetMenu(menuName="Steam Eagles/Tiles/Damaged WallTile")]
    public class DamagedWallTile : RepairableTile
    {
        [Required]
        public DamageableTile repairedVersion;
        public override DamageableTile GetRepairedTileVersion()
        {
            return repairedVersion;
        }

        public override bool CanTileBePlacedInRoom(Room room)
        {
            return room.buildLevel != BuildLevel.NONE;
        }

        public override BuildingLayers GetLayer() => BuildingLayers.WALL;

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            throw new System.NotImplementedException();
        }
    }
}