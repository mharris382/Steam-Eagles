using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items.SaveLoad
{
    public abstract class PersistentInventoryBase : MonoBehaviour
    {
        public abstract string InventoryGroupKey { get; }
        public abstract string UniqueInventoryID { get; }
        public abstract int NumberOfSlots { get; }


        [SerializeField] private Inventory targetInventory;

        private Inventory _inventory;

        public Inventory Inventory
        {
            get
            {
                if (_inventory == null)
                {
                    if(targetInventory!=null)
                        _inventory = targetInventory;
                    else
                    {
                        _inventory = gameObject.GetComponent<Inventory>();
                        if(_inventory==null)
                            _inventory = gameObject.AddComponent<Inventory>();
                    }
                }
                return _inventory;
            }
        }

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

            if(_inventory == null)
                _inventory = targetInventory!=null ? targetInventory : gameObject.AddComponent<Inventory>();
            PersistentInventoryManager.Instance.RegisterPersistentInventoryForLoad(this);
            InitSlots();
        }

        private void OnDestroy()
        {
            PersistentInventoryManager.Instance.UnregisterPersistentInventory(this);
        }

        public abstract void InitSlots();
        public abstract InventorySlot GetSlotFor(int slotNumber);


        
        
        public void LoadFromSaveData((string itemID, int itemAmount)[] toArray)
        {
            Debug.Assert(Inventory != null, $"Inventory {name} is null", this);
            for (int i = 0; i < toArray.Length; i++)
            {
                var itemId = toArray[i].itemID;
                var itemAmount = toArray[i].itemAmount;
                var slot = GetSlotFor(i);
                Debug.Assert(slot != null, $"Couldn't find slot for  {i}, on Inventory {this.name}",this);
                //has item and item is already loaded
                if (!string.IsNullOrEmpty(itemId) && itemId.IsItemLoaded())
                {
                    slot.IsSlotLoading = false;
                    var item = itemId.GetItem();
                    SetItemStack(slot, item, itemAmount, i);
                }
                //has item but item is not loaded
                else if (!string.IsNullOrEmpty(itemId))
                {
                    //TODO: trigger item load operation
                    slot.IsSlotLoading = true;
                    var i1 = i;
                    StartCoroutine(itemId.LoadItem(load => SetItemStack(slot, load, itemAmount, i1)));
                }
            }

            void SetItemStack(InventorySlot slot, ItemBase item, int itemAmount, int i)
            {
                Debug.Assert(slot.SetItemStack(new ItemStack(item, itemAmount)),
                    $"Failed to set item stack: {item.name} x {itemAmount}, in slot {i} on inventory {name}", Inventory);
            }
        }
    }
}