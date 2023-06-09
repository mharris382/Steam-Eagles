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


        public IEnumerable<Tool> GetTools()
        {
            yield return craftToolSlot.tool;
            yield return buildToolSlot.tool;
            yield return destructToolSlot.tool;
            yield return repairToolSlot.tool;
        }

        public void Awake()
        {
            
        }
    }
}