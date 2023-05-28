namespace Buildings.Tiles
{
    [System.Obsolete("Use DamageableTile to provide a damaged version of the tile")]
    public abstract class RepairableTile : EditableTile
    {
        public abstract DamageableTile GetRepairedTileVersion();
    }
}