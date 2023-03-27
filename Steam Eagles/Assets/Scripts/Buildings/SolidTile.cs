namespace Spaces
{
    public class SolidTile : EditableTile
    {
        public override bool CanTileBeDisconnected()
        {
            return true;
        }
    }
}