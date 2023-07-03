using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace Tools.UI
{
    public interface IRecipeCategoryUIController
    {
        IObservable<string> OnCategoryChange { get; }
        IObservable<Recipe> OnRecipeChange { get; }
        IObservable<(string category, Recipe recipe)> OnRecipeChangeWithCategory { get; }
        bool IsInitialized { get; }
        string CurrentCategory { get; set; }
        Recipe GetCurrentRecipeForCategory(string category);
        List<Recipe> GetRecipes(string category);
        IList<string> GetCategories();
        void SelectCategory(string category);
        void SelectRecipe(Recipe recipe);
        void SelectNextCategory();
        void SelectPreviousCategory();
        void SelectNextRecipe();
        void SelectPreviousRecipe();
    }


    public interface IRecipeUser
    {

    }


    /// <summary>
    /// manages the recipe categories UI stream
    /// </summary>
    public class RecipeCategoryUIController : PCUI, IRecipeCategoryUIController
    {
        public float recipeCycleSelectDelay = 0.1f;
        private ToolState _toolStates;

        private StringReactiveProperty _category = new StringReactiveProperty();
        private List<string> _categories = new List<string>();
        private Dictionary<string, List<Recipe>> _recipes = new Dictionary<string, List<Recipe>>();
        private Dictionary<string, IntReactiveProperty> _selectedIndex = new Dictionary<string, IntReactiveProperty>();
        private ReadOnlyReactiveProperty<Recipe> _recipe;

        [AssetList(AutoPopulate = true)] public Recipe[] recipes;

        private PlayerInput _input;


        public Recipe Recipe => _recipe == null ? null : _recipe.Value;


        private int SelectedCategoryIndex => _categories.IndexOf(_category.Value);


        public IObservable<string> OnCategoryChange => _category;

        public IObservable<Recipe> OnRecipeChange => _recipe;

        public IObservable<(string category, Recipe recipe)> OnRecipeChangeWithCategory =>
            _category.CombineLatest(_recipe, (c, r) => (c, r));


        public bool IsInitialized => _recipe != null;

        public string CurrentCategory
        {
            get => _category.Value;
            set => SelectCategory(value);
        }

        public List<Recipe> GetRecipes(string category)
        {
            if(_recipes.ContainsKey(category) == false) return new List<Recipe>();
            return _recipes[category];
        }
        
        public IList<string> GetCategories() => _categories;

        public void SelectRecipe(Recipe recipe)
        {
            if (_category.Value != recipe.category)
            {
                SelectCategory(recipe.category);
            }

            int i = _recipes[CurrentCategory].IndexOf(recipe);
            Debug.Assert(i >= 0);
            _selectedIndex[CurrentCategory].Value = i;
        }
        public void SelectCategory(string category)
        {
            if (_selectedIndex.ContainsKey(category) == false) return;
            _category.Value = category;
        }

        public void SelectNextCategory()
        {
            var index = SelectedCategoryIndex;
            index += 1;
            index %= _categories.Count;
            SelectCategory(_categories[index]);
        }
    
        public void SelectPreviousCategory()
        {
            var index = SelectedCategoryIndex;
            if(index == 0) index = _categories.Count - 1;
            else index--;
            SelectCategory(_categories[index]);
        }

        public void SelectNextRecipe()
        {
            var index = _selectedIndex[CurrentCategory].Value + 1;
            index %= _recipes[CurrentCategory].Count;
            _selectedIndex[CurrentCategory].Value = index;
        }
        
        public void SelectPreviousRecipe()
        {
            var index = _selectedIndex[CurrentCategory].Value - 1;
            if(index < 0) index = _recipes[CurrentCategory].Count - 1;
            _selectedIndex[CurrentCategory].Value = index;
        }

        public override void SetPC(PCInstance pc)
        {
            _toolStates = pc.character.GetComponent<ToolState>();
            _input = pc.input.GetComponent<PlayerInput>();
            _recipe = _category.Select(c => _selectedIndex[c].StartWith(_selectedIndex[c].Value)).Switch().Select(i => _recipes[_category.Value][i]).ToReadOnlyReactiveProperty();
            BuildRecipeTables();
            BuildUI();
            // Observable.EveryUpdate().Subscribe(_ =>
            // {
            //     int rSelect = _toolStates.Inputs.SelectRecipe;
            //     int cSelect = _toolStates.Inputs.SelectTool;
            //     if (rSelect != 0)
            //     {
            //         if(rSelect > 0) SelectNextCategory();
            //         else SelectPreviousCategory();
            //     }
            //     if(cSelect != 0)
            //     {
            //         if(cSelect > 0) SelectNextRecipe();
            //         else SelectPreviousRecipe();
            //     }
            //     
            // }).AddTo(this);
            _recipe.Subscribe(t => _toolStates.Recipe = t).AddTo(this);
            _recipe.Subscribe(t => _toolStates.RecipeName = t.name).AddTo(this);
            _category.Subscribe(c => _toolStates.RecipeCategory = c).AddTo(this);
        }

        private void BuildRecipeTables()
        {
            _recipes.Clear();
            _selectedIndex.Clear();
            _categories.Clear();

            foreach (var recipe in recipes)
            {
                if (!_recipes.TryGetValue(recipe.category, out var list))
                {
                    list = new List<Recipe>();
                    _recipes.Add(recipe.category, list);
                    _selectedIndex.Add(recipe.category, new IntReactiveProperty(0));
                }

                list.Add(recipe);
            }
        }

        private void BuildUI()
        {
            
        }

        public Recipe GetCurrentRecipeForCategory(string category)
        {
            if (_selectedIndex.ContainsKey(category))
            {
                return _recipes[category][_selectedIndex[category].Value];
            }

            return null;
        }
    }
}