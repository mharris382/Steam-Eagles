using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Item", order = 0)]
    public class Item : ItemBase
    {
        public bool isStackable;
        [ShowIf(nameof(isStackable))]
        public int maxStack = 1;

        public override int MaxStackSize => isStackable ? maxStack : 1;
    }
}