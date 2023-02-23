using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Steam Eagles/Items/Recipe")]
    public class Recipe : ScriptableObject
    {
        [HorizontalGroup("Recipe", width:0.2f, marginLeft:15)]
        [PreviewField(height:150)]
        [HideLabel]
        public Sprite icon;
        [HorizontalGroup("Recipe", width:0.8f)]
        [TableList(AlwaysExpanded = true)]
        public List<ItemStack> components;
    }
}