using System;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Items
{
    /// <summary>
    /// this is used to associate the item created from a recipe with that recipe for the purpose of deconstructing
    /// the recipe into it's component parts 
    /// </summary>
    public class RecipeInstance : MonoBehaviour
    {
        [SerializeField] private RecipeReference recipe;

        private Recipe _loadedRecipe;

        private IEnumerator Start()
        {
            var op = recipe.LoadAssetAsync<Recipe>();
            yield return op;
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedRecipe = op.Result;
                if (recipe == null) Debug.LogError($"Recipe is null from {name}", this);
            }
            else
                Debug.LogError($"Failed to load recipe {name}", this);
        }


        public bool IsLoaded => _loadedRecipe;
    }
}