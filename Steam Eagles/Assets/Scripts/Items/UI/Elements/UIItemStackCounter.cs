using UnityEngine;

namespace Items.UI
{
    [RequireComponent(typeof(Counter))]
    public class UIItemStackCounter : UIItemDisplayElement
    {
        private Counter _counter;
        private Counter Counter => _counter ? _counter : _counter = GetComponent<Counter>();
        public override void DisplayItemStack(ItemStack itemStack)
        {
            if (itemStack.IsEmpty)
            {
                Counter.Countable = null;
            }
            else
            {
                Counter.Countable = itemStack;
            }
        }
    }
}