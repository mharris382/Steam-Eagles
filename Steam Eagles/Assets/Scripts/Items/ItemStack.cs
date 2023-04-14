using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [InlineProperty(LabelWidth = 45)]
    [System.Serializable]
    public struct ItemStack : ICountable,
        IEquatable<ItemStack>,
        IComparable<ItemStack>
    {
        [ShowInInspector, HideLabel]
        public string ItemName
        {
            get => item == null ? "Empty" : item.itemName;
        }
        [HideLabel, LabelWidth(45)] public ItemBase item;
        
        [PropertyRange(1, nameof(MaxAmount))]
        [LabelText("Count"), LabelWidth(45),SerializeField] private int itemCount;

        public int MaxAmount => item == null ? 1 : item.MaxStackSize;
        public ItemStack(ItemBase item, int itemCount)
        {
            this.item = item;
            this.itemCount = itemCount;
        }

        public int Count => itemCount;

        public string Key => Item.itemName.Replace(" ", "");

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
                        _nullItem.name = _nullItem.itemName = "Null Item";
                        _nullItem.description = "This is a null item. It is used to represent an empty item slot.";
                    }

                    return _nullItem;
                }

                return item;
            }
        }

        public bool IsEmpty => item == null || itemCount <= 0;

        public void SetCount(int amount)
        {
            this.itemCount = amount;
        }
        
        
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

        public override string ToString()
        {
            if(IsEmpty) return "Empty";
            if(item.IsStackable) return $"{item.itemName} ({itemCount})";
            return item.itemName;
        }

        public static implicit operator ItemStack(ItemBase item) => new ItemStack(item, 1);
    }
}