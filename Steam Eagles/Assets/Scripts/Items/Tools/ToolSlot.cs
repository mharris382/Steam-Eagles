using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Items
{
    
    public class ToolSlot : MonoBehaviour
    {
        public Tool tool;
        public bool IsEmpty => tool == null;
        
        private ReactiveProperty<Tool> _tool;
        public Tool Tool
        {
            get => _tool.Value;
            set => _tool.Value = value;
        }

        public IObservable<Tool> OnToolChanged => _tool;

        private void Awake()
        {
            _tool = new ReactiveProperty<Tool>(tool);
            
            //make sure that the serialized value is updated when the reactive property changes
            _tool.Subscribe(t => this.tool = t).AddTo(this);
        }
    }
}