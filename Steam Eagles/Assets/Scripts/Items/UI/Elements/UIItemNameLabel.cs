using TMPro;
using UnityEngine;

namespace Items.UI
{
    public class UIItemNameLabel : UIItemDisplayElement<TextMeshProUGUI>
    {
        public override void DisplayItemStack(ItemStack itemStack)
        {
            if (itemStack.IsEmpty)
            {
                Component.text = "";
            }
            else
            {
                Component.text = itemStack.item.itemName;
            }
        }
    }
}