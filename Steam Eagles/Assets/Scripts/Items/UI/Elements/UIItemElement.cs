using UI;

namespace Items.UI
{
    public abstract class UIItemElement : UIElement, IItemUI
    {

        private ItemBase _item;
        
        
        public ItemBase Item
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

        public abstract void OnItemChanged(ItemBase item);
    }
}