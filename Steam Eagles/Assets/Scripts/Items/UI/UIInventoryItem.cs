using System.Collections;
using UnityEngine;

namespace Items.UI
{
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
                icon.Item = itemSlot.itemStack.item;
                stackCounter.IsVisible = itemSlot.itemStack.item.IsStackable;
                stackCounter.Countable = itemSlot.itemStack;
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
                bool stackable = inventorySlot.itemStack.item.IsStackable;
                if (stackable)
                {
                    stackCounter.Countable = inventorySlot.itemStack;
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