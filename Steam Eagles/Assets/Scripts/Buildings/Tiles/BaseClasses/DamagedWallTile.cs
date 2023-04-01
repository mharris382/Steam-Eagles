using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Tiles
{
    [CreateAssetMenu(menuName="SteamEagles/Tiles/Damaged WallTile")]
    public class DamagedWallTile : RepairableTile
    {
        [Required]
        public DamageableTile repairedVersion;
        public override DamageableTile GetRepairedTileVersion()
        {
            return repairedVersion;
        }
    }
}