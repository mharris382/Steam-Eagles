namespace Items.UI
{
    public interface IDisplayItemStackUI
    {
        bool IsVisible { get; set; }
        void DisplayItemStack(ItemStack itemStack);
    }
}