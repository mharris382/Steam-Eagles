using System.Collections.Generic;

namespace CoreLib.MyEntities.PersistentData
{
    [System.Serializable]
    public class InventoryData
    {
        public readonly string inventoryID;
        public List<ItemSlots> itemSlots = new List<ItemSlots>();
        public InventoryData(string inventoryID)
        {
            this.inventoryID = inventoryID;
        }
        
        
        [System.Serializable]
        public class ItemSlots
        {
            public string itemID;
            public int itemAmount;
        }
    }
}