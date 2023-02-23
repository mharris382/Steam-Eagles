using TMPro;

namespace Items.UI
{
    public class UIItemDescriptionText : UIItemDisplayElement<TextMeshProUGUI>
    {
        public override void DisplayItemStack(ItemStack itemStack)
        {
            if (itemStack.IsEmpty)
            {
                Component.text = "";
            }
            else
            {
                Component.text = itemStack.Item.description;
            }
        }
    }
}