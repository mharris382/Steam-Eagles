using System;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace Items.SaveLoad
{
    public class PersistentInventoryManager : Singleton<PersistentInventoryManager>
    {
        public override bool DestroyOnLoad => false;
        
        
        private Dictionary<string, PersistentInventoryBase> _inventories = new Dictionary<string, PersistentInventoryBase>();

        public void UnregisterPersistentInventory(PersistentInventoryBase inventory)
        {
            if (_inventories.ContainsKey(inventory.UniqueInventoryID))
            {
                _inventories.Remove(inventory.UniqueInventoryID);
            }
            //TODO: copy inventory out of the runtime inventory and store it in format for saving
            //throw new NotImplementedException();
        }
        public void RegisterPersistentInventoryForLoad(PersistentInventoryBase inventory)
        {
            if (_inventories.ContainsKey(inventory.UniqueInventoryID))
            {
                Debug.LogWarning("Already Registered Inventory with ID: " + inventory.UniqueInventoryID);
                return;
            }
            _inventories.Add(inventory.UniqueInventoryID, inventory);
        }
        
        /// <summary>
        /// access the persistent inventory by its unique ID for loading purposes
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PersistentInventoryBase GetPersistentInventory(string id)
        {
            if (_inventories.ContainsKey(id))
            {
                return _inventories[id];
            }
            return null;
        }
    }
}