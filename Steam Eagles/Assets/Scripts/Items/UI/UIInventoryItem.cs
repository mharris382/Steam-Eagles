using System.Collections;
using UnityEngine;

namespace Items.UI
{
    [System.Obsolete("Replacing this with a generic display system. Use UIItem")]
    public class UIInventoryItem : MonoBehaviour
    {
        public ItemIcon icon;
        public Counter stackCounter;

        private bool isCleared;
        public void DisplaySlot(InventorySlot itemSlot)
        {
            if (itemSlot.IsEmpty)
            {
                Clear();
            }
            else
            {
                isCleared = false;
                icon.IsVisible = true;
                icon.Item = itemSlot.ItemStack.item;
                stackCounter.IsVisible = itemSlot.ItemStack.item.IsStackable;
                stackCounter.Countable = itemSlot.ItemStack;
                StartCoroutine(DelayedDisplay(itemSlot));
            }
        }

        IEnumerator DelayedDisplay(InventorySlot inventorySlot)
        {
            yield return null;
            isCleared = false;
            while (!isCleared)
            {
                icon.IsVisible = true;
                bool stackable = inventorySlot.ItemStack.item.IsStackable;
                if (stackable)
                {
                    stackCounter.Countable = inventorySlot.ItemStack;
                    stackCounter.IsVisible = true;
                }
                else
                {
                    stackCounter.IsVisible = false;
                }
                 
                yield return null;
            }
        }
        
        

        public void Clear()
        {
            isCleared = true;
            icon.IsVisible = false;
            stackCounter.IsVisible = false;
        }
    }
}