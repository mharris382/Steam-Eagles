using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public interface IInventory
    {
        
    }

    public class ItemContainer : MonoBehaviour
    {
         string containerName = "Container";
         int numberOfSlots = 9;

        private void Awake()
        {
            if (string.IsNullOrEmpty(containerName)) 
                containerName = "Container";
            
            for (int i = 0; i < numberOfSlots; i++)
            {
                var slot = new GameObject($"{containerName} Slot " + i);
                slot.transform.SetParent(transform);
            }
        }
    }
    
    public class Inventory : MonoBehaviour, IInventory
    {
        public Transform slotParent;


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
        
        public IEnumerable<ItemStack> items
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

        public void AddItem(ItemBase item, int amount)
        {
            throw new NotImplementedException();
        }

        public int GetItemCount(ItemBase item)
        {
            int cnt = 0;
            foreach (var itemStack in items)
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
            int cnt = GetItemCount(item);
            if(amount > cnt)
                return false;
            throw new NotImplementedException();
            return false;
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
            int cnt = GetItemCount(itemBase);
            return cnt + countToAdd <= itemBase.MaxStackSize;
        }
    }


    public class InventoryTester : MonoBehaviour
    {
        public List<ItemStack> inventoryItems = new List<ItemStack>();
        public Inventory inventory;
    }
}