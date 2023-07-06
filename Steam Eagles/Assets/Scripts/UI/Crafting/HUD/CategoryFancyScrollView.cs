using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
using FancyScrollView;
using Sirenix.OdinInspector;
using UniRx;

namespace UI.Crafting.HUD
{
    public class CategoryFancyScrollView : FancyScrollView<Recipe, CategoryScrollContext>
    {
        [Required] public GameObject cellPrefab;
        [SerializeField,Required] private Scroller scroller;
        
        private UICrafting _crafting;
        protected override GameObject CellPrefab => cellPrefab;


        private void Awake()
        {
            _crafting = GetComponentInParent<UICrafting>();
        }

        private void Start()
        {
            
            Context.Init(_crafting);
             _crafting.recipes.OnCategoryChanged.Select(t => _crafting.recipes.categories[t]).Subscribe(OnCategoryChanged).AddTo(this);
            _crafting.recipes.OnRecipeChanged.Subscribe(OnRecipeChanged).AddTo(this);
        }

        void OnRecipeChanged(Recipe recipe)
        {
            var currentRecipes =  _crafting.recipes.CurrentRecipes.Value;
            var currentRecipe = _crafting.recipes.CurrentRecipe.Value;
            scroller.ScrollTo(currentRecipes.IndexOf(currentRecipe), 0.3f);
        }
        void OnCategoryChanged(string category)
        {
            var currentRecipes =  _crafting.recipes.CurrentRecipes.Value;
            var currentRecipe = _crafting.recipes.CurrentRecipe.Value;
            scroller.SetTotalCount(currentRecipes.Count);
            UpdateContents(currentRecipes);
            scroller.SetTotalCount(currentRecipes.Count);
            scroller.JumpTo(currentRecipes.IndexOf(currentRecipe));
            
        }
    }

    public class CategoryScrollContext
    {
        private UICrafting _crafting;
        private bool _inited;
        
        public List<Recipe> CurrentRecipes => _inited ? _crafting.recipes.CurrentRecipes?.Value : null;
        public Recipe CurrentRecipe => _inited ? _crafting.recipes.CurrentRecipe?.Value :null;
        public void Init(UICrafting crafting)
        {
            _crafting = crafting;
            _inited = true;
        }
    }
}