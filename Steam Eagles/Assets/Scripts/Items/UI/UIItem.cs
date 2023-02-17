using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Items.UI
{
    /// <summary>
    /// non reactive version of the item display UI
    /// </summary>
    public class UIItem : MonoBehaviour
    {
        [ChildGameObjectsOnly]
        public List<UIItemDisplayElement> uiElements = new List<UIItemDisplayElement>();

        private InventorySlot _slot;
        private IDisposable _slotDisposable;


        public bool isCleared => _slot != null;

        private void Awake()
        {
            
        }

        protected virtual IEnumerable<IDisplayItemStackUI> GetUIElements()
        {
            return uiElements;
        }


        public bool IsVisible
        {
            set
            {
                foreach (var displayItemStackUI in GetUIElements())
                {
                    displayItemStackUI.IsVisible = value;
                }
            }
        }
        
        
        
        public void DisplaySlot(InventorySlot slot)
        {
            if (_slotDisposable != null)
            {
                Clear();
            }
            
            var itemStack = slot.ItemStack;
            var item = itemStack.item;
            this._slot = slot;
            var coroutine = StartCoroutine(UpdateDisplay());
            _slotDisposable = Disposable.Create(() => {
                StopCoroutine(coroutine);
                _slot = null;
                _slotDisposable = null;
            });
        }

        private IEnumerator UpdateDisplay()
        {
            Debug.Assert(_slot != null, "Why was this coroutine started before assigning a slot?");
            while (_slot != null)
            {
                DisplayStack(_slot.ItemStack);
                yield return null;
            }
        }

        public void DisplayStack(ItemStack stack)
        {
            foreach (var displayItemStackUI in GetUIElements())
            {
                displayItemStackUI.IsVisible = true;
                displayItemStackUI.DisplayItemStack(stack);
            }
        }

        public void Clear()
        {
            if (_slotDisposable != null)
            {
                _slotDisposable.Dispose();
                _slotDisposable = null;
            }
        }
    }
}