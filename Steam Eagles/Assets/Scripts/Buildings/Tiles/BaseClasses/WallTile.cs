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
    }
}