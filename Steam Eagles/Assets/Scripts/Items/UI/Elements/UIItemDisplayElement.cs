using UI;
using UnityEngine;

namespace Items.UI
{
    public abstract class UIItemDisplayElement : UIElement, IDisplayItemStackUI
    {
        public abstract void DisplayItemStack(ItemStack itemStack);
    }
    
    public abstract class UIItemDisplayElement<T> : UIItemDisplayElement where T : Component
    {
        private T _component;
        public T Component
        {
            get
            {
                if (_component == null)
                {
                    _component = GetComponent<T>();
                    if (_component == null) _component = GetComponentInChildren<T>();
                }
                return _component;
            }
        }
    }
}