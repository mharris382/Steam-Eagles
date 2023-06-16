using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(InventorySlot))]
    public class ToolSlot : MonoBehaviour
    {
        public Tool tool;
        public bool IsEmpty => tool == null;
        
        private ReactiveProperty<Tool> _tool;
        private InventorySlot _slot;

        
        public Tool Tool
        {
            get => _tool.Value;
            set => _tool.Value = value;
        }

        public IObservable<Tool> OnToolChanged
        {
            get
            {
                if(_tool.HasValue)
                    return _tool.StartWith(_tool.Value);
                else
                {
                    return _tool;
                }
            }
        }

        private void Awake()
        {
            _slot = GetComponent<InventorySlot>();
            
            var tool = _slot.ItemStack.item as Tool;
            
            _tool = new ReactiveProperty<Tool>(tool);
            _slot.onStackChanged.AsObservable().Select(t => t.Item as Tool).Subscribe(t => _tool.Value = t).AddTo(this);
        }

        private IEnumerator Start()
        {
            while (_slot.IsSlotLoading)
            {
                yield return null;
            }
            
            
        }
    }
}