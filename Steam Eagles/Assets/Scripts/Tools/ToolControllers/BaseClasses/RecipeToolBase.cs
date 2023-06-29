using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using Buildings.Tiles;
using Characters;
using Items;
using Tools.RecipeTool;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tools.BuildTool
{
    public abstract class RecipeToolBase<T> : ToolControllerBase where T:UnityEngine.Object
    {
        [SerializeField] private float recipeSwitchRate = .2f;
        [SerializeField] private bool faceDirectionOfCharacter = true;
        
        
        
        private float _selectRecipeTime;
        private List<Recipe> _recipes;
        private RecipeSelector _recipeSelector;
        private PreviewResourceHandler<T> _previewResourceHandler;
        
        [System.Obsolete("Use PreviewResourceHandler instead")] private Dictionary<Recipe, RecipePreviewer> _recipePreviewers = new Dictionary<Recipe, RecipePreviewer>();
        [System.Obsolete("Use PreviewResourceHandler instead")] private RecipePreviewer _currentPreview;
        
        
        

        public Recipe CurrentRecipe => _recipes == null ? null : _recipeSelector.SelectedRecipe.Value;
        protected List<Recipe> Recipes => _recipes;

        protected void Update()
        {
            if (!HasResources()) return;
            if(this.Building == null || !this.Building.IsFullyLoaded)return;
            if (base.Recipe == null) Debug.Assert(_recipeSelector != null, "_recipeSelector != null", this);
            if (_recipeSelector == null) throw new NullReferenceException();
            if(_previewResourceHandler ==null)throw new NullReferenceException();
            if (CurrentRecipe == null)
            {
                TryToSelectRecipe();
                return;
            }

            if (CheckForRecipeSwitch(this.ToolState))
            {
                return;
            }

            
            if (!_previewResourceHandler.IsPreviewReady)//wait for recipe preview to finish loading resources
            {
                SetPreviewVisible(false);
                return;
            }
            SetPreviewVisible(true);
            
            bool isFlipped = GetFlipped();
            var layer = GetTargetLayer();
            string errorMessage = "";

            AimHandler.UpdateAimPosition(layer);
            
            
            
            //NOTE: update preview before checking if action is valid (because preview is used to check if action is valid)
            UpdatePreview(Building,  isFlipped, _previewResourceHandler.Preview);
            
            OnUpdate(Building, isFlipped);

            if (!CheckIfActionIsValid(ref errorMessage))
            {
                SharedData.ErrorMessage.Value = errorMessage;
                return;
            }
            SharedData.ErrorMessage.Value = "";
            
            //do base last because base checks for tool switches so that will disable the next update loop
        }

        bool CheckIfActionIsValid(ref string errorMessage)
        {
            if (!CurrentRecipe.IsAffordable(Inventory))
            {
                errorMessage = "Cannot afford recipe";
                return false;
            }
            if(IsPlacementInvalid(ref errorMessage))
            {
                return false;
            }
            return true;
        }

        private bool GetFlipped() => faceDirectionOfCharacter && CharacterState.FacingRight;
        protected override void OnRoomChanged(Room room) => Debug.LogWarning($"{nameof(RecipeToolBase<T>)} throw new System.NotImplementedException();");

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


        public sealed override void SetRecipeSelector(RecipeSelector recipeSelector)
        {
            if (_recipeSelector != null)
            {
                Debug.LogWarning("Called SetRecipeSelector twice", this);
            }
            _recipeSelector = recipeSelector;
            _previewResourceHandler = new PreviewResourceHandler<T>(this, recipeSelector);
            _recipeSelector.SelectedRecipe.Subscribe(OnRecipeChanged).AddTo(this);;
        }


        protected virtual void OnRecipeChanged(Recipe recipe) => Debug.Log("TODO: Update previewer");

        private void TryToSelectRecipe()
        {
            Debug.Assert(_recipeSelector != null, "_recipeSelector != null", this);
            _recipeSelector.SelectAny();
            Debug.Assert(Recipe.SelectedRecipe.Value != null, "Recipe.SelectedRecipe.Value != null", this);
        }


        protected virtual void OnUpdate(Building building, bool isFlipped) { }

        public override void OnToolEquipped()
        {
            SetPreviewVisible(true);
            base.OnToolEquipped();
        }

        public override void OnToolUnEquipped()
        {
            SetPreviewVisible(false);
            base.OnToolUnEquipped();
        }


        public sealed override bool UsesRecipes(out List<Recipe> recipes)
        {
            Debug.Assert(tool != null, $"tool == null", this);
            Debug.Assert(tool.Recipes != null, $"{tool}.Recipes == null", tool);
            _recipes = recipes = new List<Recipe>(GetRecipes());
            return true;
        }

        
        
        
        public abstract void UpdatePreview(Building building, bool isFlipped, T previewResource);

        protected abstract IEnumerable<Recipe> GetRecipes();
        public abstract override void SetPreviewVisible(bool visible);
        
        
        public abstract bool IsPlacementInvalid( ref string errorMessage);
    }
}