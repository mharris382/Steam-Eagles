namespace Items
{
    [System.Serializable]
    public struct ItemStack : ICountable
    {
        public Item item;
        public int itemCount;
        public int Count => itemCount;
    }
}