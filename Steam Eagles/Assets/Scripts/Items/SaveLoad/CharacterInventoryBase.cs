using System;

namespace Items.SaveLoad
{
    public abstract class CharacterInventoryBase : PersistentInventoryBase
    {
        public string CharacterName => tag;
        
        public override string InventoryGroupKey => CharacterName;
        
        
    }
}