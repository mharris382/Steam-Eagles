using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [InlineProperty(LabelWidth = 45)]
    [System.Serializable]
    public struct ItemStack : ICountable, IEquatable<ItemStack>, IComparable<ItemStack>
    {
        [HideLabel, LabelWidth(45)]
        public ItemBase item;
        [LabelText("Count"), LabelWidth(45)]
        public int itemCount;

        public ItemStack(ItemBase item, int itemCount)
        {
            this.item = item;
            this.itemCount = itemCount;
        }

        public int Count => itemCount;


        private static ItemBase _nullItem;
        private static ItemStack _empty;
        public static ItemStack Empty => _empty;
        public ItemBase Item
        {
            get
            {
                if (item == null)
                {
                    if (_nullItem == null)
                    {
                        _nullItem = ScriptableObject.CreateInstance<ItemBase>();
                        _nullItem.name = _nullItem.itemName =  "Null Item";
                        _nullItem.description = "This is a null item. It is used to represent an empty item slot.";
                    }

                    return _nullItem;
                }
                return item;
            }
        }
        
        public bool IsEmpty => item == null || itemCount <= 0;

        public bool Equals(ItemStack other)
        {
            return Equals(item, other.item) && itemCount == other.itemCount;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(item, itemCount);
        }

        public int CompareTo(ItemStack other)
        {
            return itemCount.CompareTo(other.itemCount);
        }
    }
}