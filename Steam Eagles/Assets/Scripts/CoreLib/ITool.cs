using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    public interface ITool
    {
        // ReSharper disable once InconsistentNaming
        public string name { get; }

        public bool UsesRecipes();
    }

    public class CoreRecipe
    {
        public readonly IIconable recipeIcon;
        public readonly List<(string item, int count)> components;

        public Sprite RecipeIcon => recipeIcon.GetIcon();
        public string RecipeName => recipeIcon.name;
        public CoreRecipe(IIconable recipeIcon, List<(string item, int count)> components)
        {
            this.recipeIcon = recipeIcon;
            this.components = new List<(string, int)>(components);
            
        }
    }

   
    
}