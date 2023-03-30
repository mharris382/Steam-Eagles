//using PhysicsFun.DynamicBlocks;

namespace Buildings.Tiles
{
    public abstract class EditableTile : PuzzleTile
    {
        //public DynamicBlock dynamicBlock;
        public override bool CanTileBeDisconnected()
        {
            //Debug.Assert(dynamicBlock!=null, $"Tile {name} is missing a DynamicBlock!", this);
            return true;
        }
    }
}