using System.Collections.Generic;
using System.Text;
using Items;
using UniRx;
using UnityEngine;

namespace Tools.RecipeTool
{
    public class RecipeSelector
    {
        private ReactiveProperty<Recipe> _selectedRecipe = new ReactiveProperty<Recipe>();
        public ReadOnlyReactiveProperty<Recipe> SelectedRecipe => _selectedRecipe.ToReadOnlyReactiveProperty();
        private List<Recipe> _recipes;

        public void SelectRecipe(Recipe recipe)
        {
            if (_recipes.Contains(recipe))
            {
                _selectedRecipe.Value = recipe;
            }
        }
        public RecipeSelector(List<Recipe> recipes)
        {
            Debug.Assert(recipes.Count > 0, "recipes.Count > 0");
            _recipes = recipes;
            _selectedRecipe.Value = _recipes[0];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Selected {_selectedRecipe.Value}");
            foreach (var recipe in _recipes)
            {
                sb.AppendLine(recipe.ToString());
            }
            return base.ToString();
        }
    }
}