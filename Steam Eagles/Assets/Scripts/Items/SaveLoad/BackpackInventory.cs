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
            //_slots = new List<InventorySlot>(numberOfSlots);
            //for (int i = 0; i < _slots.Count; i++)
            //{
            //    CreateSlot(i);
            //}
        }

        private void CreateSlot(int i)
        {
            //var newSlotGo = new GameObject($"{InventoryGroupKey}_{UniqueInventoryID}_Slot#{i}", typeof(InventorySlot));
            //newSlotGo.transform.parent = transform;
            //_slots.Add(newSlotGo.GetComponent<InventorySlot>());
        }

        /// <summary>
        /// zero based index to retrieve the inventory slot in inventory 
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <returns></returns>
        public override InventorySlot GetSlotFor(int slotNumber)
        {
            return transform.GetChild(slotNumber).GetComponent<InventorySlot>();
            if ( _slots == null || slotNumber >= _slots.Count-1)
            {
                RebuildInventory(slotNumber);
            }
            try
            {
                slotNumber = Mathf.Clamp(slotNumber,0, numberOfSlots - 1);
                if (slotNumber >= transform.childCount)
                {
                    for (int i = transform.childCount; i <= slotNumber; i++)
                    {
                        var go = new GameObject("", typeof(InventorySlot));
                        go.transform.parent = transform;
                        _slots.Add(go.GetComponent<InventorySlot>());
                    }
                }
                return transform.GetChild(slotNumber).GetComponent<InventorySlot>();
                return _slots[slotNumber];
            }
            catch (IndexOutOfRangeException e)
            {
                
                Debug.LogError($"Got index out of range exception for backpack inventory: {name}",this);
                return null;
            }
        }

        private void RebuildInventory(int size)
        {
            
            int oldSize = transform.childCount;
            if (size > oldSize-1)
            {
                for (int i = oldSize + 1; i < size; i++)
                {
                    CreateSlot(i);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            _slots = new List<InventorySlot>(size);
            for (int i = 0; i < size; i++)
            {
                _slots.Add(transform.GetChild(i).GetComponent<InventorySlot>());
            }
        }
    }
}