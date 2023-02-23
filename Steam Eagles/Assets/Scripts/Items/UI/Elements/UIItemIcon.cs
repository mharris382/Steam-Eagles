using UnityEngine;
using UnityEngine.UI;
namespace Items.UI
{
    public class UIItemIcon : UIItemDisplayElement
    {
        public Image icon;


        public override void DisplayItemStack(ItemStack itemStack)
        {
            if (itemStack.IsEmpty)
            {
                icon.enabled = false;
            }
            else
            {
                icon.enabled = true;
                icon.sprite = itemStack.item.icon;
            }
        }
    }
}