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

        protected override void OnUpdate()
        {
            if (_currentPreview == null)
            {
                TryUpdatePreview();
            }
            var selectedPositionWS = transform.TransformPoint(this.ToolState.AimPositionLocal);
            _currentPreview.UpdatePreview(this.targetBuilding, selectedPositionWS, out var isValid);
            if (isValid && ToolState.Inputs.UsePressed)
            {
                if(Time.realtimeSinceStartup - _timeBuildTime > buildRate)
                {
                    _timeBuildTime = Time.realtimeSinceStartup;
                    var newMachine = _currentPreview.Build(targetBuilding);
                }
            }
            base.OnUpdate();
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
        }
    }
}