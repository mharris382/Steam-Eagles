using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items.SaveLoad
{
    public abstract class PersistentInventoryBase : MonoBehaviour
    {
        public abstract string InventoryGroupKey { get; }
        public abstract string UniqueInventoryID { get; }
        public abstract int NumberOfSlots { get; }


        [SerializeField]
        private Inventory targetInventory;
        
        private Inventory _inventory;
        

        protected string SaveKey => $"{InventoryGroupKey}_{UniqueInventoryID}";
        
        protected virtual void Awake()
        {
            void GetAllSlots(List<InventorySlot> inventorySlots)
            {
                for (int i = 0; i < NumberOfSlots; i++)
                {
                    inventorySlots.Add(GetSlotFor(i));
                    Debug.Assert(inventorySlots[i] != null, $"Inventory {name} Found Null Slot at {i}", this);
                }
            }

            _inventory = targetInventory!=null ? targetInventory : gameObject.AddComponent<Inventory>();
            InitSlots();
            var itemStacks = InventorySaveLoader.LoadedInventorySave[SaveKey];
            
            
            
            //found the expected number of slots in save
            if (itemStacks.stacks.Count == NumberOfSlots)
            {
                
            }
            //found more slots in save than expected
            else if (itemStacks.stacks.Count > NumberOfSlots)
            {
                
            }
            // expected more slots than expected
            else
            {
                
            }
            
            //get the slots to load items into
            List<InventorySlot> slots = new List<InventorySlot>(NumberOfSlots);
            GetAllSlots(slots);
        }

        public abstract void InitSlots();
        public abstract InventorySlot GetSlotFor(int slotNumber);
    }
}