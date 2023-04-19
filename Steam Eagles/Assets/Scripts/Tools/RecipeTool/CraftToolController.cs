using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using CoreLib;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.RecipeTool
{
    public class CraftToolController : ToolControllerBase
    {
        [SerializeField] private PreviewConfig config;
        
        [BoxGroup("Debugging")]
        [ShowInInspector, HideInEditorMode, ReadOnly]
        private List<Recipe> _recipes;

        [BoxGroup("Debugging")] [ShowInInspector]
        private RecipeSelector _recipeSelector;
        
        [BoxGroup("Debugging")] [ShowInInspector]
        private RecipePreviewer _currentPreview;
        
        [BoxGroup("Debugging")] [ShowInInspector]
        public Recipe CurrentRecipe => _recipes == null ? null : _recipeSelector.SelectedRecipe.Value;


        protected override void OnStart()
        {
            Debug.Log($"{Recipe.SelectedRecipe.Value} is the Recipe Selected",this);
        }

        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(CraftToolController)} throw new System.NotImplementedException();");
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.Recipe;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            _recipes = recipes = new List<Recipe>();
            foreach (var toolRecipe in tool.Recipes)
            {
                _recipes.Add(toolRecipe);
            }
            return true;
        }

        public override void SetRecipeSelector(RecipeSelector recipeSelector)
        {
            _recipeSelector = recipeSelector;
            _recipeSelector.SelectedRecipe.Subscribe(OnRecipeChanged).AddTo(this);
        }

        void OnRecipeChanged(Recipe recipe)
        {
            Debug.Log("TODO: Update previewer");
        }
        
        
        private void Update()
        {
            if (!HasResources() || _recipeSelector == null)
            {
                if (_recipeSelector == null)
                {
                    Debug.Log(base.Recipe);
                }
                return;
            }

            
            if (CurrentRecipe == null)
            {
                if (_recipes.Count > 0)
                {
                    _recipeSelector.SelectRecipe(_recipes.FirstOrDefault(t => t != null));
                }
                return;
            }

            if (_currentPreview == null)
            {
                _currentPreview = new RecipePreviewer(this, this.config, CurrentRecipe);
            }

            var selectedPositionWS = transform.TransformPoint(this.ToolState.AimPositionLocal);
            _currentPreview.UpdatePreview(this.targetBuilding, selectedPositionWS);
        }
    }
}