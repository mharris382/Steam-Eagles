using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items.UI
{
    public class TestCounter : MonoBehaviour
    {
        [Required] public List<UIItemElement> itemUIItemElements;
        [Required] public Counter counter;
        public List<ItemStack> items;

        private int _index;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _index++;
                if (_index >= items.Count) _index = 0;
                DisplayItem(items[_index]);
            }
        }

        private void DisplayItem(ItemStack itemStack)
        {
            counter.Countable = itemStack;
            counter.IsVisible = true;
            foreach (var uiItemElement in itemUIItemElements)
            {
                uiItemElement.IsVisible = true;
                uiItemElement.Item = itemStack.item;
            }
        }
    }
}