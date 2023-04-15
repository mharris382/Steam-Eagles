using System;
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
        
        
        public void Awake()
        {
            
        }
    }
}