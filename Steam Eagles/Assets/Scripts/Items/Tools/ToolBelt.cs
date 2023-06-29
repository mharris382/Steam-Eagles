using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UniRx;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(ToolSlots))]
    public class ToolBelt : MonoBehaviour
    {
        ToolSlots _slots;
        

        public void Awake()
        {
            _slots = GetComponent<ToolSlots>();
            _slots.onToolEquipped.AsObservable().Subscribe(OnToolEquipped).AddTo(this);
        }


        void OnToolEquipped(Tool tool)
        {
            Debug.Log($"Equipped {tool.name}");
        }
    }
}