using System;
using System.Collections.Generic;
using Buildings.Rooms;
using Characters;
using Items;
using Tools.RecipeTool;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public abstract class RecipeToolBase : ToolControllerBase
    {
        [SerializeField] private float recipeSwitchRate = .2f;
        [SerializeField] private bool faceDirectionOfCharacter = true;
        
        
        
        private float _selectRecipeTime;
        private List<Recipe> _recipes;
        private RecipeSelector _recipeSelector;

        private Dictionary<Recipe, RecipePreviewer> _recipePreviewers = new Dictionary<Recipe, RecipePreviewer>();
        private RecipePreviewer _currentPreview;
        
        
        

        public Recipe CurrentRecipe => _recipes == null ? null : _recipeSelector.SelectedRecipe.Value;
        protected List<Recipe> Recipes => _recipes;

        protected override void OnRoomChanged(Room room) => Debug.LogWarning($"{nameof(RecipeToolBase)} throw new System.NotImplementedException();");
        public abstract void SetPreviewVisible(bool visible);
        
        protected void Update()
        {
            if (!HasResources()) return;
            if (base.Recipe == null) Debug.Assert(_recipeSelector != null, "_recipeSelector != null", this);
            if (_recipeSelector == null) throw new NullReferenceException();

            if (CurrentRecipe == null)
            {
                TryToSelectRecipe();
                return;
            }

            if (CheckForRecipeSwitch(this.ToolState))
            {
                return;
            }
            
            OnUpdate(GetFlipped());
            
            //do base last because base checks for tool switches so that will disable the next update loop
        }


        private bool GetFlipped() => faceDirectionOfCharacter && CharacterState.FacingRight;


        private bool CheckForRecipeSwitch(ToolState toolState)
        {
            if (Time.realtimeSinceStartup - _selectRecipeTime > recipeSwitchRate && toolState.Inputs.SelectRecipe != 0)
            {
                _selectRecipeTime = Time.realtimeSinceStartup;
                if (toolState.Inputs.SelectRecipe > 0)
                {
                    _recipeSelector.Next();
                }
                else
                {
                    _recipeSelector.Previous();
                }
                
                return true;
            }
            return false;
        }


        public override void SetRecipeSelector(RecipeSelector recipeSelector)
        {
            if (_recipeSelector != null)
            {
                Debug.LogWarning("Called SetRecipeSelector twice", this);
            }
            _recipeSelector = recipeSelector;
            _recipeSelector.SelectedRecipe.Subscribe(OnRecipeChanged).AddTo(this);;
        }

        protected virtual void OnRecipeChanged(Recipe recipe) => Debug.Log("TODO: Update previewer");

        private void TryToSelectRecipe()
        {
            Debug.Assert(_recipeSelector != null, "_recipeSelector != null", this);
            _recipeSelector.SelectAny();
            Debug.Assert(Recipe.SelectedRecipe.Value != null, "Recipe.SelectedRecipe.Value != null", this);
        }


        protected virtual void OnUpdate(bool isFlipped)
        {
            
        }

        protected abstract IEnumerable<Recipe> GetRecipes();


        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            Debug.Assert(tool != null, $"{tool.name} == null", this);
            Debug.Assert(tool.Recipes != null, $"{tool}.Recipes == null", tool);
            _recipes = recipes = new List<Recipe>(GetRecipes());
            return true;
        }
    }
}