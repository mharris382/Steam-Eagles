using System;
using System.Collections.Generic;
using CoreLib;
using Items;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.RecipeTool
{
    public class CraftToolController : RecipeToolBase
    {
        
        [SerializeField] private PreviewConfig config;
        [SerializeField] private float buildRate = 1;
        private float _timeBuildTime;
        
        private Dictionary<Recipe, RecipePreviewer> _recipePreviewers = new Dictionary<Recipe, RecipePreviewer>();
        private RecipePreviewer _currentPreview;
        
        

        public override ToolStates GetToolState() => ToolStates.Recipe;

        protected override IEnumerable<Recipe> GetRecipes() => tool.Recipes;

        public override void SetPreviewVisible(bool visible)
        {
            if (_currentPreview != null)
            {
                _currentPreview.SetVisible(visible);
            }
        }

        protected override void OnRecipeChanged(Recipe recipe)
        {
            Debug.Log($"Changed recipe to {recipe}",this);
            _currentPreview?.SetVisible(false);
            _currentPreview = null;
            TryUpdatePreview();
            Debug.Assert(_currentPreview != null, "_currentPreview == null",this);
        }

        protected override void OnUpdate(bool isFlipped)
        {
            if (_currentPreview == null)
            {
                TryUpdatePreview();
            }
            _currentPreview.SetVisible(true);
            var selectedPositionWS = transform.TransformPoint(this.ToolState.AimPositionLocal);
            _currentPreview.UpdatePreview(this.targetBuilding, selectedPositionWS, out var isValid, isFlipped);
            TryBuildMachine(isValid);
            base.OnUpdate(isFlipped);
        }

        private void TryBuildMachine(bool isValid)
        {
            if (isValid && ToolState.Inputs.UsePressed)
            {
                if (Time.realtimeSinceStartup - _timeBuildTime > buildRate)
                {
                    BuildMachine();
                }
            }
        }

        private void BuildMachine()
        {
            _timeBuildTime = Time.realtimeSinceStartup;
            var newMachine = _currentPreview.Build(targetBuilding);
            if (newMachine != null)
            {
                //newMachine.machineAddress = Recipe.SelectedRecipe.Value.InstanceReference.ToString();
                Debug.Log($"Built machine: {newMachine} with {newMachine.machineAddress}", this);
            }
        }


        private void OnDisable()
        {
            _currentPreview?.SetVisible(false);
        }


        private void TryUpdatePreview()
        {
            if(_recipePreviewers.TryGetValue(CurrentRecipe, out var previewer))
            {
                _currentPreview = previewer;
                return;
            }
            _currentPreview = new RecipePreviewer(this, this.config, CurrentRecipe);
            _recipePreviewers.Add(CurrentRecipe, _currentPreview);
            _currentPreview.SetVisible(true);
        }
    }
}