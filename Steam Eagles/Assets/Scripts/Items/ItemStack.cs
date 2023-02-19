using Sirenix.OdinInspector;
using UnityEngine;

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


        private static ItemBase _nullItem;
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
        
    }
}