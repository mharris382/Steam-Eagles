using UnityEngine;
using UnityEngine.UI;

namespace Items.UI
{
    [RequireComponent(typeof(Counter))]
    public class UIItemStackCounter : UIItemDisplayElement
    {
        private Counter _counter;
        private Counter Counter => _counter ? _counter : _counter = GetComponent<Counter>();
        private Image _image;
        private Image Image => _image ? _image : _image = GetComponent<Image>();
        public override void DisplayItemStack(ItemStack itemStack)
        {
            if (itemStack.IsEmpty || !itemStack.item.IsStackable)
            {
                Counter.Countable = null;
                Image.enabled = false;
                IsVisible = false;
            }
            else
            {
                Counter.Countable = itemStack;
                Counter.UpdateCounter();
                Image.enabled = true;
                IsVisible = true;
            }
        }
    }
}