using System;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;
using Zenject;

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
        [Inject] void InjectMe(ToolManager.Factory prefabHelper)
        {
            var prefabLoader = prefabHelper.Create(transform);
            foreach (var slot in GetToolSlots())
            {
                slot.InjectMe(prefabLoader);
            }
        }

        public IEnumerable<IObservable<Tool>> GetTools()
        {
            yield return craftToolSlot.OnToolChanged;
            yield return buildToolSlot.OnToolChanged;
            yield return destructToolSlot.OnToolChanged;
            yield return repairToolSlot.OnToolChanged;
        }

        public IEnumerable<ToolSlot> GetToolSlots()
        {
            yield return craftToolSlot;
            yield return buildToolSlot;
            yield return destructToolSlot;
            yield return repairToolSlot;
        }

        public void Awake()
        {
                
        }
    }
}