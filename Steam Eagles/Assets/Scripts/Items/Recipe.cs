using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace Items
{
    [CreateAssetMenu(menuName = "Steam Eagles/Items/Recipe")]
    public class Recipe : ScriptableObject
    {
        [HorizontalGroup("Recipe", width:0.2f, marginLeft:15)]
        [HideLabel]
        public Sprite icon;
        [HorizontalGroup("Recipe", width:0.8f)]
        [TableList(AlwaysExpanded = true)]
        public List<ItemStack> components;
        
        
        
        public bool HasComponent(ItemBase item)
        {
            return components.Any(t => t.Item == item);
        }
        
        
    }


}