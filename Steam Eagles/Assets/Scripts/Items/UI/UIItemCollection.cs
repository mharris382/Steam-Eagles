using System.Collections.Generic;
using UnityEngine;

namespace Items.UI
{
    public class UIItemCollection : MonoBehaviour
    {
        public bool fixedSize;
        public int size = 10;

        public string slotKey = "ItemSlot";

        public Transform itemContainer;

        public bool IsVisible
        {
            set
            {
                foreach (var componentsInChild in GetComponentsInChildren<UIItem>())
                {
                    componentsInChild.IsVisible = value;
                }
            }
        }
        
        public void LinkContainer(List<InventorySlot> slots)
        {
            int count = fixedSize ? size : slots.Count;
            for (int i = 0; i < count; i++)
            {
                var uiSlot =  UIInventoryItemSlots.Instance.GetUIItem(slotKey);
                uiSlot.transform.SetParent(itemContainer);
                if(slots.Count > i)
                {
                    //show item
                    uiSlot.DisplaySlot(slots[i]);
                }
                else
                {
                    //show empty slot
                    uiSlot.Clear();
                }
                uiSlot.IsVisible = true;
            }
        }

        public void PopulateContainer(List<ItemStack> items)
        {
            int count = fixedSize ? size : items.Count;
            for (int i = 0; i < count; i++)
            {
                var uiSlot =  UIInventoryItemSlots.Instance.GetUIItem(slotKey);
                uiSlot.transform.SetParent(itemContainer);
                if(items.Count > i)
                {
                    //show item
                    uiSlot.DisplayStack(items[i]);
                }
                else
                {
                    //show empty slot
                    uiSlot.Clear();
                }
                uiSlot.IsVisible = true;
            }
        }

        public void ClearContainer()
        {
            var uiElements = itemContainer.GetComponentsInChildren<UIItem>();
            foreach (var uiElement in uiElements)
            {
                uiElement.IsVisible = false;
                UIInventoryItemSlots.Instance.ReturnUIItemSlot(slotKey, uiElement);
            }
        }
    }
}