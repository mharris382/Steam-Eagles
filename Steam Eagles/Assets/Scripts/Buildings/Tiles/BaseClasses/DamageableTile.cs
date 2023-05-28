namespace Buildings.Tiles
{
    public abstract class DamageableTile : EditableTile
    {
        [System.Obsolete]
        public abstract RepairableTile GetDamagedTileVersion();
    }
}