using Sirenix.OdinInspector;

namespace Items
{
    [InlineProperty(LabelWidth = 45)]
    [System.Serializable]
    public struct ItemStack : ICountable
    {
        [HideLabel, LabelWidth(45)]
        public ItemBase item;
        [LabelText("Count"), LabelWidth(45)]
        public int itemCount;
        public int Count => itemCount;
        
        
        public bool IsEmpty => item == null || itemCount <= 0;
        
    }
}