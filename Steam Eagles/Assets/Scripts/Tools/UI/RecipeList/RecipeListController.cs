using System;
using System.Collections.Generic;
using Items;
using Items.UI.HUDScrollView;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.UI
{
    /// <summary>
    /// controls the HUDFancyScrollView for a given recipe category
    /// </summary>
    public class RecipeListController : MonoBehaviour
    {
        [Required] public HUDFancyScrollView scrollView;
        [SerializeField] private Events events;
        
        [Serializable]
        private class Events
        {
            public UnityEvent<bool> onFocusChanged;
        }
        
        public CanvasGroup canvasGroup;

        private ReadOnlyReactiveProperty<bool> _isFocused;
        private ReadOnlyReactiveProperty<Recipe> _currentRecipe;

        private string _categoryName;
        private bool _initialized;

        [ShowInInspector, BoxGroup("Debug"), ReadOnly]
        public string CategoryName
        {
            get { return _categoryName; }
            private set
            {
                _categoryName = value;
                name = $"RecipeListController: {value}";
            }
        }

        [ShowInInspector, BoxGroup("Debug")]
        public bool IsFocused
        {
            get => _initialized ? _isFocused.Value : false;
            set
            {
                
            }
        }

        [ShowInInspector, BoxGroup("Debug")]
        public Recipe CurrentRecipe => _initialized ? _currentRecipe.Value : null;
        
        
        
        public void AssignAll(
            List<Recipe> recipes, 
            string category,
            Recipe recipe,
            IObservable<Recipe> recipeStream)
        {
            _categoryName = category;

            _isFocused = recipeStream.Select(t => recipes.Contains(recipe)).ToReadOnlyReactiveProperty();
            _currentRecipe = recipeStream.Where(t => recipes.Contains(recipe)).ToReadOnlyReactiveProperty();
            
            int index = recipes.IndexOf(recipe);
            Debug.Assert(index >= 0);
            scrollView.UpdateData(recipes);


            _initialized = true;
            if (canvasGroup != null) _isFocused.Subscribe(focused => canvasGroup.alpha = focused ? 1 : 0).AddTo(this);
            
            _isFocused.Subscribe(focused => events.onFocusChanged?.Invoke(focused)).AddTo(this);
            _currentRecipe.Subscribe(r => scrollView.Select(r)).AddTo(this);
        }

        
    }
}