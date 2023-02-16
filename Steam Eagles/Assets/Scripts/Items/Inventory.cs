using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class InventorySlot : MonoBehaviour
    {
        public IInventory inventory
        {
            get;
            set;
        }
        public ItemStack itemStack;
    }

    public interface IInventory
    {
        
    }
    public class Inventory : MonoBehaviour, IInventory
    {
        public Transform slotParent;




        public IEnumerable<ItemStack> items
        {
            get
            {
                for (int i = 0; i < slotParent.childCount; i++)
                {
                    var child = slotParent.GetChild(i);
                    var slot = child.GetComponent<InventorySlot>();
                    slot.inventory = this;
                    yield return slot.itemStack;
                }
            }
        }
        
        
        
        public int SlotCount => slotParent.childCount;
        public void AddSlot()
        {
            var newSlotGo = new GameObject("Slot");
            newSlotGo.transform.SetParent(slotParent);
            newSlotGo.AddComponent<InventorySlot>();
        }
        
        
        
        
        public void AddItem(Item item, int amount)
        {
            
        }

        public int GetItemCount(Item item)
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
        public bool RemoveItem(Item item, int amount)
        {
            int cnt = GetItemCount(item);
            if(amount > cnt)
                return false;
            if (amount > item.MaxStackSize)
            {
                int stacksToRemove = amount / item.MaxStackSize;
                int itemsLeft = amount % item.MaxStackSize;
            }
            return false;
        }
    }


    public class InventoryTester : MonoBehaviour
    {
        public List<ItemStack> inventoryItems = new List<ItemStack>();
        public Inventory inventory;
    }
}