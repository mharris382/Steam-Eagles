using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Tool", order = 0)]
    public class Tool : ItemBase
    {
        public override int MaxStackSize => 1;
        public override bool IsStackable => false;


        [ToggleGroup(nameof(usesRecipes), "Recipes")]
        [SerializeField] private bool usesRecipes;
        [ToggleGroup(nameof(usesRecipes)), InlineEditor(Expanded = true)]
        [SerializeField] private List<Recipe> recipes;
    }
}