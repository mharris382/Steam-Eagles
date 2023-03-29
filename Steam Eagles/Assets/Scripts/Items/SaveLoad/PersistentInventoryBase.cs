using System;
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
            _inventory = targetInventory!=null ? targetInventory : gameObject.AddComponent<Inventory>();
            InitSlots();
            var itemStacks = InventorySaveLoader.LoadedInventorySave[SaveKey];
            Debug.Assert(itemStacks.Count == NumberOfSlots, "itemStacks.Count != NumberOfSlots", this);
        }

        public abstract void InitSlots();
        public abstract InventorySlot GetSlotFor(int slotNumber);
    }
}