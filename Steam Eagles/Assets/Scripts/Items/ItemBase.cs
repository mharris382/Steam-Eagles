using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public abstract class ItemBase : ScriptableObject
    {
        [PreviewField(Height = 200, Alignment = ObjectFieldAlignment.Left)]
        [Required] public Sprite icon;
        public string itemName;
        [Multiline] public string description;

        public bool IsStackable => MaxStackSize > 1;
        public abstract int MaxStackSize { get; }
    }
}