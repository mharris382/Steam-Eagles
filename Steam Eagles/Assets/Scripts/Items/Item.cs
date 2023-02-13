using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Item", order = 0)]
    public class Item : ScriptableObject
    {
        
        [Required] public Sprite icon;
        public string itemName;
        [Multiline] public string description;
        
        public bool isStackable;
        [ShowIf(nameof(isStackable))]
        public int maxStack = 1;
    }
}