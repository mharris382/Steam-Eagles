using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Item", order = 0)]
    public class Item : ItemBase
    {
        [VerticalGroup("Item Info/Name_Desc")]
        [ToggleGroup("Item Info/Name_Desc/isStackable")]
        public bool isStackable;
        [ToggleGroup("Item Info/Name_Desc/isStackable")]
        [ShowIf(nameof(isStackable))]
        public int maxStack = 1;

        
        public override int MaxStackSize => isStackable ? maxStack : 1;
        public override ItemType ItemType => ItemType.RESOURCE;
    }
}