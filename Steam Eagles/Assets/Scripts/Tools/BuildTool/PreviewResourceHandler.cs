using System;
using System.Collections.Generic;
using Items;
using Tools.RecipeTool;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tools.BuildTool
{
    public class PreviewResourceHandler<T> where T:UnityEngine.Object
    {
        Dictionary<Recipe, Recipe.Loader<T>> _seenRecipes = new Dictionary<Recipe, Recipe.Loader<T>>();
        
        public bool HasPreview { get; private set; }

        public bool IsPreviewReady
        {
            get
            {
                if (!HasPreview)
                {
                    return false;
                }

                if (_currentPreview == null)
                {
                    return false;
                }

                if (!_currentPreview.IsLoaded)
                {
                    return false;
                }
                
                return true;
            }   
        }

        public T Preview => IsPreviewReady ? _currentPreview.LoadedObject : null;
        
        private Recipe.Loader<T> _currentPreview;
        private IDisposable _disposable;
        private readonly MonoBehaviour caller;

        public PreviewResourceHandler(MonoBehaviour caller, RecipeSelector recipeSelector)
        {
            this.caller = caller;
            HasPreview = false;
            SubscribeToRecipeSelector(recipeSelector);
        }
        
        public void SubscribeToRecipeSelector(RecipeSelector recipeSelector)
        {
            _disposable = recipeSelector.SelectedRecipe.Subscribe(recipe =>
            {
                if (recipe == null)
                {
                    HasPreview = false;
                    return;
                }
                AddRecipe(recipe);
                _currentPreview = _seenRecipes[recipe];
                HasPreview = true;
            });
        }

        
        private void AddRecipe(Recipe recipe)
        {
            if(_seenRecipes.ContainsKey(recipe))return;
            var loader = recipe.GetLoader<T>(caller);
            _seenRecipes.Add(recipe, loader);
            HasPreview = true;
        }
        //public bool IsPreviewReady
    }
}