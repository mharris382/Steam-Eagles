using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        
        
        [SerializeField, EnumPaging] private RecipeType recipeType;
        
        [ShowIf(nameof(UseInstanceReference))]
        [SerializeField] private RecipeInstanceReference instanceReference;
        
        [ShowIf(nameof(UseTileReference))]
        [SerializeField] private TileReference tileReference;
        
        bool UseTileReference => recipeType == RecipeType.TILE;
        bool UseInstanceReference => recipeType == RecipeType.MACHINE;
        public enum RecipeType
        {
            TILE,
            MACHINE
        }
        
        
        public bool HasComponent(ItemBase item) => components.Any(t => t.Item == item);

        public AssetReference InstanceReference
        {
            get
            {
                switch (recipeType)
                {
                    case RecipeType.TILE:
                        return tileReference;
                    case RecipeType.MACHINE:
                        return instanceReference;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
    }


}