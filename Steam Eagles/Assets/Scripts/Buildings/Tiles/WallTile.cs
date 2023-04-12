using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Tiles
{
    [CreateAssetMenu(menuName="SteamEagles/Tiles/WallTile")]
    public class WallTile : DamageableTile
    {
        [Required]
        public RepairableTile damagedVersion;
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