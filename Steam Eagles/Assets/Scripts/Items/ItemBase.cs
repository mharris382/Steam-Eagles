using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public abstract class ItemBase : ScriptableObject
    {
        [HorizontalGroup("h1")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 300)]
        [HideLabel]
        [Required] public Sprite icon;

        
        [VerticalGroup("h1/v1")]
        [LabelText("Name"),LabelWidth(80)]
        public string itemName;

        [VerticalGroup("h1/v1")]
        [HideLabel, LabelWidth(80)]
        [Multiline(5)]
        public string description;
        
        [Serializable]

        public class DropInfo
        {
            [ToggleLeft]
            public bool canDrop;
            [ ShowIf(nameof(canDrop))]
            public string dropKey;
        }


        [SerializeField] private DropInfo dropInfo;

        [FoldoutGroup("Lore",expanded:false)]
        [Tooltip("a narrative description of the item, which may or may not directly pertain to gameplay but connects the item to the story and reveals more details about the world's lore")]
        [Multiline(5),HideLabel] public string lore;


        public virtual bool IsStackable => MaxStackSize > 1;
        public abstract int MaxStackSize { get; }

        public abstract ItemType ItemType { get; }
        
        public bool canDrop => dropInfo.canDrop;

        public string dropKey => dropInfo.dropKey;
    }
}