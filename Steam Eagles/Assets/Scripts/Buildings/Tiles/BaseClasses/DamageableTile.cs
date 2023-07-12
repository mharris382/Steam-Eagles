namespace Buildings.Tiles
{
    public abstract class DamageableTile : EditableTile
    {
        
        public abstract RepairableTile GetDamagedTileVersion();
    }
}