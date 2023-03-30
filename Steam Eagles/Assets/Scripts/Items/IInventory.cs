namespace Items
{
    public interface IInventory : IReadOnlyInventory
    {
        bool CanAddItem(ItemBase itemBase, int countToAdd = 1);
        
        bool AddItem(ItemBase item, int amount);

        bool CanRemoveItem(ItemBase itemBase, int countToRemove = 1);

        /// <summary>
        /// tries to remove the given amount of items from the inventory,
        /// if there are not enough items nothing will be removed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        bool RemoveItem(ItemBase item, int amount);
    }
    
    
    public interface IReadOnlyInventory
    {
        System.Collections.Generic.IEnumerable<InventorySlot> itemSlots { get; }
        int SlotCount { get; }
        int GetItemCount(ItemBase item);
    }
}