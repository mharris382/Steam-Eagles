using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public abstract class ItemBase : ScriptableObject
    {
        [HorizontalGroup("Item Info", width:0.2f), HideLabel]
        [PreviewField(Height = 200, Alignment = ObjectFieldAlignment.Left)]
        [Required] public Sprite icon;
        
        [LabelText("Name")]
        [HorizontalGroup("Item Info", width:0.8f, marginLeft:15, LabelWidth = 54)]
        [VerticalGroup("Item Info/Name_Desc")]
        public string itemName;
        
        [HideLabel]
        [HorizontalGroup("Item Info", width:0.8f, LabelWidth = 54)]
        [VerticalGroup("Item Info/Name_Desc")]
        [Multiline(5)] public string description;

        [VerticalGroup("Item Info/Name_Desc")] [HorizontalGroup("Item Info/Name_Desc/Dropping", width: 0.5f)]
        [ToggleLeft]
        public bool canDrop;
        [HorizontalGroup("Item Info/Name_Desc/Dropping", width: 0.7f), ShowIf(nameof(canDrop))]
        public string dropKey;
        public bool IsStackable => MaxStackSize > 1;
        public abstract int MaxStackSize { get; }
    }
}