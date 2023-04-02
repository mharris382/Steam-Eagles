using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items.SaveLoad
{
    public class BackpackInventory : CharacterInventoryBase
    {
        public int numberOfSlots = 15;
        
        private List<InventorySlot> _slots = new List<InventorySlot>();
        
        
        public override string UniqueInventoryID => "Backpack";
        public override int NumberOfSlots => numberOfSlots;

        public override void InitSlots()
        {
            _slots = new List<InventorySlot>(numberOfSlots);
            for (int i = 0; i < _slots.Count; i++)
            {
                var newSlotGo = new GameObject($"{InventoryGroupKey}_{UniqueInventoryID}_Slot#{i}", typeof(InventorySlot));
                newSlotGo.transform.parent = transform;
                _slots.Add(newSlotGo.GetComponent<InventorySlot>());
            }
        }

        /// <summary>
        /// zero based index to retrieve the inventory slot in inventory 
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <returns></returns>
        public override InventorySlot GetSlotFor(int slotNumber)
        {
            try
            {
                slotNumber = Mathf.Clamp(0, numberOfSlots - 1, slotNumber);
                return _slots[slotNumber];
            }
            catch (IndexOutOfRangeException e)
            {
                
                Debug.LogError($"Got index out of range exception for backpack inventory: {name}",this);
                return null;
            }
        }
    }
}