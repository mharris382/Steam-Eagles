namespace Buildings.Tiles
{
    public abstract class RepairableTile : EditableTile
    {
        public abstract DamageableTile GetRepairedTileVersion();
    }
}