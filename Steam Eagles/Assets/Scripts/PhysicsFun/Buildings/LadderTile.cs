using UnityEngine;

namespace Spaces
{
    [CreateAssetMenu(menuName = "Steam Eagles/Tiles/Ladder Tile")]
    public class LadderTile : EditableTile
    {
        public override bool CanTileBeDisconnected()
        {
            return false;
        }
    }
}