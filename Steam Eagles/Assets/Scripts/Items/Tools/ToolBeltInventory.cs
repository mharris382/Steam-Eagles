using System;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(Inventory))]
    public class ToolBeltInventory : MonoBehaviour
    {
        Inventory _inventory;
        
        public Inventory Inventory => _inventory ??= GetComponent<Inventory>();
        
        public ToolSlot craftToolSlot;
        public ToolSlot buildToolSlot;
        public ToolSlot destructToolSlot;
        public ToolSlot repairToolSlot;
        ToolBelt _toolBelt;


        public IEnumerable<IObservable<Tool>> GetTools()
        {
            yield return craftToolSlot.OnToolChanged;
            yield return buildToolSlot.OnToolChanged;
            yield return destructToolSlot.OnToolChanged;
            yield return repairToolSlot.OnToolChanged;
        }
        
        

        public void Awake()
        {
                
        }
    }
}