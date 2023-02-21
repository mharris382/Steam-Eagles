using System.Collections.Generic;
using System.Linq;
using Items;

namespace Tests.ItemTests
{
    public class InventorySlotsInfo
    {
        private readonly List<InventorySlot> _emptySlots;
        private readonly List<InventorySlot> _nonEmptySlots;
        private readonly Dictionary<ItemBase, List<InventorySlot>> _itemSlots;
        private readonly Dictionary<InventorySlot, ItemBase> _slotItems;
        public List<InventorySlot> EmptySlots => _emptySlots;
        public List<InventorySlot> NonEmptySlots => _nonEmptySlots;
        
        public Dictionary<ItemBase, List<InventorySlot>> ItemSlots => _itemSlots;

        public InventorySlotsInfo(IEnumerable<InventorySlot> slots)
        {
            _emptySlots  = new List<InventorySlot>();
            _itemSlots = new Dictionary<ItemBase, List<InventorySlot>>();
            _slotItems = new Dictionary<InventorySlot, ItemBase>();
            _nonEmptySlots = new List<InventorySlot>();
            foreach (var inventorySlot in slots)
            {
                if (inventorySlot.IsEmpty)
                {
                    _emptySlots.Add(inventorySlot);
                    _slotItems.Add(inventorySlot, null);
                }
                else
                {
                    if (!_itemSlots.ContainsKey(inventorySlot.ItemStack.Item))
                    {
                        _itemSlots.Add(inventorySlot.ItemStack.Item, new List<InventorySlot>());
                    }
                    _itemSlots[inventorySlot.ItemStack.Item].Add(inventorySlot);
                    _slotItems.Add(inventorySlot, inventorySlot.ItemStack.Item);
                    _nonEmptySlots.Add(inventorySlot);
                }
            }

            foreach (var inventorySlot in slots)
            {
                //TODO: add event listeners, and update info when slots change
            }
        }

        public IEnumerable<InventorySlot> GetSlotsContainingItem(ItemBase itemBase)
        {
            if(_itemSlots.ContainsKey(itemBase))
                return _itemSlots[itemBase];
            return null;
        }
        
        /// <summary>
        /// return the amount of items which can be added to the stacks in the inventory only in slots already containing the item
        /// </summary>
        /// <param name="itemBase"></param>
        /// <returns></returns>
        public int GetStackableSpace(ItemBase itemBase)
        {
            if (_itemSlots.ContainsKey(itemBase))
            {
                int space = 0;
                foreach (var inventorySlot in _itemSlots[itemBase])
                {
                    space += inventorySlot.ItemStack.item.MaxStackSize - inventorySlot.ItemStack.Count;   
                }
                return space;
            }
            return 0;
        }

        public IEnumerable<InventorySlot> GetInventorySlotsWhichCanAdd(ItemBase itemBase)
        {
            if (itemBase.IsStackable && _itemSlots.ContainsKey(itemBase))
            {
                foreach (var inventorySlot in _itemSlots[itemBase])
                {
                    if (!inventorySlot.IsFull)
                        yield return inventorySlot;
                }
            }

            foreach (var inventorySlot in _emptySlots)
            {
                //slot should still be empty, but check anyway in case it was changed
                if (inventorySlot.IsEmpty)
                {
                    yield return inventorySlot;
                }
                
            }
        }

        public int CountSlotsContainingItem(ItemBase itemBase)
        {
            var slots = GetSlotsContainingItem(itemBase);
            if (slots == null) return 0;
            return slots.Count();
        }
    }
}