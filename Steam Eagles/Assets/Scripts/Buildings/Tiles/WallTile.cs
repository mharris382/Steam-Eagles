using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Tiles
{
    [CreateAssetMenu(menuName="SteamEagles/Tiles/WallTile")]
    public class WallTile : DamageableTile
    {
        
        public RepairableTile damagedVersion;

        public override bool CanTileBePlacedInRoom(Room room)
        {
            return room.buildLevel != BuildLevel.NONE;
        }

        public override RepairableTile GetDamagedTileVersion()
        {
            return damagedVersion;
        }

        public override BuildingLayers GetLayer() => BuildingLayers.WALL;

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            throw new System.NotImplementedException();
        }
    }
}