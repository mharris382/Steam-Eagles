using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Items.UI
{
    public class ItemIcon : HUDElement
    {
        public Image icon;


    }

    public abstract class UIItemElement : UIElement, IItemUI
    {

        private Item _item;
        
        
        public Item Item
        {
            get => _item;
            set
            {
                if (_item != value)
                {
                    _item = value;
                    OnItemChanged();
                }
            }
        }

        private void OnItemChanged()
        {
            if (Item != null)
            {
                OnItemChanged(Item);
            }
            else
            {
                IsVisible = false;
            }
        }

        protected sealed override void OnBecameVisible()
        {
            if (Item == null)
            {
                IsVisible = false;
                return;
            }
            else
            {
                OnItemChanged(Item);
            }
        }

        public abstract void OnItemChanged(Item item);
    }

}