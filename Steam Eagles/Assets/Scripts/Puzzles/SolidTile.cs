namespace Spaces
{
    public class SolidTile : PuzzleTile
    {
        public override bool CanTileBeDisconnected()
        {
            return true;
        }
    }
}