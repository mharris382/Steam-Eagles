using PhysicsFun.DynamicBlocks;
using UnityEngine;

namespace Spaces
{
    public abstract class EditableTile : PuzzleTile
    {
        public DynamicBlock dynamicBlock;
        public override bool CanTileBeDisconnected()
        {
            Debug.Assert(dynamicBlock!=null, $"Tile {name} is missing a DynamicBlock!", this);
            return true;
        }
    }
}