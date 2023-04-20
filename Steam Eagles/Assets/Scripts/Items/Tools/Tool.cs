using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Tool", order = 0)]
    public class Tool : ItemBase, IIconable
    {
        public override int MaxStackSize => 1;
        public override bool IsStackable => false;


        [ToggleGroup(nameof(usesRecipes), "Recipes")]
        [SerializeField] private bool usesRecipes;
        [ToggleGroup(nameof(usesRecipes)), InlineEditor(Expanded = true)]
        [SerializeField] private List<Recipe> recipes;

        public override ItemType ItemType => ItemType.TOOL;


        public IEnumerable<Recipe> Recipes => recipes;


        
        [ValidateInput(nameof(ValidateState), "Must assign a valid tool state!")]
        public ToolStates toolState;

        public ToolControllerReference controllerPrefab;
        
        bool ValidateState(ToolStates state)
        {
            return state != ToolStates.None;
        }

        public Sprite GetIcon()
        {
            return this.icon;
        }
    }
}