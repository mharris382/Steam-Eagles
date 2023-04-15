using System;
using System.Collections.Generic;
using System.Linq;
using Tests.ItemTests;
using UnityEngine;

namespace Items
{
    public struct ItemAddedEventArgs
    {
        
    }
    
    public struct ItemRemovedEventArgs
    {
           
    }

    public class Inventory : MonoBehaviour, IInventory
    {
        public Transform slotParent;

        private static ItemStackSorter _sorter;
        private static ItemStackSorter sorter => _sorter ?? (_sorter = new ItemStackSorter());

        public bool isMain;

        public bool isToolbelt;
        
        
        
        void Start()
        {
            if (slotParent == null)
            {
                slotParent = new GameObject("Slots").transform;
                slotParent.SetParent(transform, false);
            }
        }


        public IEnumerable<InventorySlot> itemSlots
        {
            get
            {
                for (int i = 0; i < slotParent.childCount; i++)
                {
                    var child = slotParent.GetChild(i);
                    var slot = child.GetComponent<InventorySlot>();
                    slot.inventory = this;
                    yield return slot;
                }
            }
        }
        
        public IEnumerable<ItemStack> Items
        {
            get
            {
                foreach (var inventorySlot in itemSlots)
                {
                    yield return inventorySlot.ItemStack;
                }
            }
        }
        
        public int SlotCount => slotParent.childCount;

        public bool AddItem(ItemBase item, int amount)
        {
            if (!CanAddItem(item, amount))
            {
                return false;
            }
            int remaining = amount;
            var info = new InventorySlotsInfo(itemSlots);
            foreach (var inventorySlot in info.GetInventorySlotsWhichCanAdd(item))
            {
                if(remaining <= 0) break;
                int toAdd = Mathf.Min(remaining, inventorySlot.IsEmpty ? item.MaxStackSize : inventorySlot.GetRemainingStackableSpace());
                Debug.Log($"Adding {toAdd} {item.itemName} to slot {inventorySlot.transform.GetSiblingIndex()}");
                int newAmount = inventorySlot.ItemStack.Count + toAdd;
                Debug.Assert(newAmount<= item.MaxStackSize);
                inventorySlot.SetItemStack(item, newAmount);
                remaining -= toAdd;
            }
            return true;
        }

        public int GetItemCount(ItemBase item)
        {
            int cnt = 0;
            foreach (var itemStack in Items)
            {
                if (itemStack.item == item)
                {
                    cnt += itemStack.Count;
                }
            }
            return cnt;
        }
        
        
        /// <summary>
        /// tries to remove the given amount of items from the inventory,
        /// if there are not enough items nothing will be removed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(ItemBase item, int amount)
        {
            if (!CanRemoveItem(item, amount))
                return false;
            var info = new InventorySlotsInfo(itemSlots);
            var slots = info.GetSlotsContainingItem(item);
            int amtToRemove = amount;
            foreach (var slot in slots)
            {
                if (amtToRemove <= 0)
                    break;
                int amt = slot.ItemStack.Count;
                if (amt > amtToRemove)
                {
                    var newAmt = amt - amtToRemove;
                    slot.SetItemStack(new ItemStack(item, newAmt));
                    amtToRemove = 0;
                }
                else
                {
                    amtToRemove -= amt;
                    slot.Clear();
                }
            }
            return true;
        }


        public int GetEmptySlotCount()
        {
            int cnt = 0;
            foreach (var inventorySlot in itemSlots)
            {
                if (inventorySlot.IsEmpty)
                {
                    cnt++;
                }
            }
            return cnt;
        }

        
        public bool CanRemoveItem(ItemBase itemBase, int countToRemove = 1)
        {
            int cnt = GetItemCount(itemBase);
            return cnt >= countToRemove;
        }
        
        public bool CanAddItem(ItemBase itemBase, int countToAdd = 1)
        {
            //find out how many empty slots would be needed to add the items (ignoring stacks for now)
            int slotsNeeded = !itemBase.IsStackable ? countToAdd : Mathf.CeilToInt(countToAdd / (float) itemBase.MaxStackSize);
            int emptySlots = GetEmptySlotCount();
            //if there are enough empty slots, we can add the items 
            if (emptySlots > slotsNeeded)
            {
                return true;
            }

            //since there are not enough empty slots we need to check if we can stack the items in the existing slots
            if (!itemBase.IsStackable) //if the item is not stackable we can't add it
                return false;
            
            int spaceNeededRemainingAfterEmptySlotsAreFilled = countToAdd - (emptySlots * itemBase.MaxStackSize);
            
            
            var info = new InventorySlotsInfo(itemSlots);
            int stackableSpace = info.GetStackableSpace(itemBase);
            return stackableSpace > spaceNeededRemainingAfterEmptySlotsAreFilled;
        }
        
        
        /// <summary>
        /// tries to minimize the number of slots used, by combining stacks of the same item
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void MergeStacks(bool sortAfterMerge = false)
        {
            var slotInfo = new InventorySlotsInfo(this.itemSlots);
            var itemSlots = slotInfo.ItemSlots;
            
            foreach (var itemSlot in itemSlots)
            {
                var slotsWithSameItem = itemSlot.Value;
                var item = itemSlot.Key;
                MergeSlotsWithSameItem(slotsWithSameItem, item);
            }

            void MergeSlotsWithSameItem(List<InventorySlot> slotsWithSameItem, ItemBase item)
            {
                int cnt = slotsWithSameItem.Count;
                if (cnt <= 1)
                    return;
                if (!item.IsStackable)
                    return;
                int maxStackSize = item.MaxStackSize;
                int totalItemCount = 0;
                foreach (var inventorySlot in slotsWithSameItem)
                {
                    totalItemCount += inventorySlot.ItemStack.Count;
                }

                int minNumberOfSlots = totalItemCount / maxStackSize;
                int remainingItems = totalItemCount % maxStackSize;
                int numberOfSlotsNeeded = minNumberOfSlots + (remainingItems > 0 ? 1 : 0);
                for (int i = 0; i < slotsWithSameItem.Count; i++)
                {
                    var slot = slotsWithSameItem[i];
                    if (i < numberOfSlotsNeeded)
                    {
                        Debug.Log($"Merging {item.name} into slot {slot.name}");
                        int itemsInSlot = i == numberOfSlotsNeeded - 1 ? remainingItems : maxStackSize;
                        slot.SetItemStack(item, itemsInSlot);
                    }
                    else
                    {
                        Debug.Log($"Clearing Slot {slot.name}");
                        slot.Clear();
                    }
                }
            }

            if (sortAfterMerge) SortInventory();
        }

        public void SortInventory()
        {
            var itemStacks = Items.ToList();
            itemStacks.Sort(sorter);
            for (int i = 0; i < slotParent.childCount; i++)
            {
                var child = slotParent.GetChild(i);
                var slot = child.GetComponent<InventorySlot>();
                slot.inventory = this;
                slot.SetItemStack(itemStacks[i]);
            }
        }
    }

    
}