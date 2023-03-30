using System;
using System.Collections.Generic;

namespace Items
{
    public class ItemStackSorter : IComparer<ItemStack>,
        IComparer<ItemBase>,
        IComparer<ItemType>
    {
        public int Compare(ItemStack x, ItemStack y)
        {
            int itemComparison = Compare(x.item, y.item);
            if (itemComparison != 0)
                return itemComparison;
            return x.Count.CompareTo(y.Count);
        }

        public int Compare(ItemBase x, ItemBase y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return -1;
            if (ReferenceEquals(null, x)) return 1;
            int typeComparison = Compare(x.ItemType, y.ItemType);
            if (typeComparison != 0)
                return typeComparison;
            var itemNameComparison = string.Compare(x.itemName, y.itemName, StringComparison.Ordinal);
            return itemNameComparison;
        }

        public int Compare(ItemType x, ItemType y)
        {
            return x.CompareTo(y);
        }
    }
}