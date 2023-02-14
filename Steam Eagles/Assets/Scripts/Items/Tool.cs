using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Tool", order = 0)]
    public class Tool : ItemBase
    {
        public override int MaxStackSize => 1;
    }
}