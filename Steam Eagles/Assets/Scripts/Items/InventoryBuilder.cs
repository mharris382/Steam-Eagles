using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public static class InventoryBuilder
    {
        public static Inventory BuildInventory(string inventoryName, int numberOfSlots)
        {
            var inventoryGo = new GameObject(inventoryName);
            var inventory = inventoryGo.AddComponent<Inventory>();
            inventory.transform.position = Vector3.zero;

            var slotContainer = new GameObject($"{{inventoryName}} Slots").transform;
            inventory.slotParent = slotContainer;
            slotContainer.SetParent(inventoryGo.transform, false);

            for (int i = 0; i < numberOfSlots; i++)
            {
                var slotGo = new GameObject($"{inventoryName} Slot " + i);
                slotGo.transform.SetParent(slotContainer);
                var slot = slotGo.AddComponent<InventorySlot>();
                slot.inventory = inventory;
            }
            return inventory;
        }
        public static Inventory BuildInventory(string inventoryName, List<ItemStack> initialItems)
        {
            return BuildInventory(inventoryName, initialItems.Count, initialItems);
        }
        public static Inventory BuildInventory(string inventoryName, int numberOfSlots, List<ItemStack> initialItems)
        {
            var inventoryGo = new GameObject(inventoryName);
            var inventory = inventoryGo.AddComponent<Inventory>();
            inventory.transform.position = Vector3.zero;

            if(initialItems.Count > numberOfSlots)
                throw new Exception($"initialItems.Count ({initialItems.Count}) > numberOfSlots ({numberOfSlots}");
            
            var slotContainer = new GameObject($"{{inventoryName}} Slots").transform;
            inventory.slotParent = slotContainer;
            slotContainer.SetParent(inventoryGo.transform, false);

            for (int i = 0; i < numberOfSlots; i++)
            {
                var slotGo = new GameObject($"{inventoryName} Slot " + i);
                slotGo.transform.SetParent(slotContainer);
                var slot = slotGo.AddComponent<InventorySlot>();
                slot.inventory = inventory;
                if (i < initialItems.Count)
                {
                    slot.SetItemStackSafe(initialItems[i]);
                }
            }
            return inventory;
        }
    }
}