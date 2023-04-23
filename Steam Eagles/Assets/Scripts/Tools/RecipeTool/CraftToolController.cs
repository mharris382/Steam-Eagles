using System;
using System.Collections.Generic;
using Buildables;
using Buildings;
using CoreLib;
using Items;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.RecipeTool
{
    public class CraftToolController : RecipeToolBase<GameObject>
    {
        
        [SerializeField] private PreviewConfig config;
        [SerializeField] private float buildRate = 1;
        private float _timeBuildTime;

        private BuildableMachineBase _selectedMachine;
        private bool _isValid;

        [Obsolete("Use _previewers instead")] private Dictionary<Recipe, RecipePreviewer> _recipePreviewers = new Dictionary<Recipe, RecipePreviewer>();
        [Obsolete("Use _previewers instead")] private RecipePreviewer _currentPreview;

        private Dictionary<GameObject, LoadedRecipePreviewer> _previewers =
            new Dictionary<GameObject, LoadedRecipePreviewer>();

        private GameObject _lastUsedPrefab;
        private LoadedRecipePreviewer CurrentPreviewer => _lastUsedPrefab == null ? null : GetPreviewer(_lastUsedPrefab);

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        public override ToolStates GetToolState() => ToolStates.Recipe;

        protected override IEnumerable<Recipe> GetRecipes() => tool.Recipes;

        public override void SetPreviewVisible(bool visible) => CurrentPreviewer?.SetVisible(visible);


        public override void UpdatePreview(Building building, bool isFlipped,
            GameObject prefab)
        {
            if (prefab == null)
            {
                _isValid = false;
                _lastUsedPrefab = null;
                _selectedMachine = null;
                return;
            }

            if (_lastUsedPrefab != prefab && _lastUsedPrefab != null) GetPreviewer(_lastUsedPrefab).SetVisible(false);

            _lastUsedPrefab = prefab;
            
            var previewer = GetPreviewer(prefab);
            AimHandler.UpdateAimPosition(previewer.TargetLayer);
            previewer.SetVisible(true);
            previewer.UpdatePreview(building, AimHandler.HoveredPosition.Value, out _isValid, isFlipped);
            _selectedMachine = previewer.machine;
        }

        private LoadedRecipePreviewer GetPreviewer(GameObject prefab)
        {
            if (prefab == null) return null;
            if(!_previewers.TryGetValue(prefab, out var previewer))
            {
                _previewers.Add(prefab, previewer = new LoadedRecipePreviewer(config, prefab));
            }
            return previewer;
        }

        

        protected override void OnUpdate(Building building, bool isFlipped)
        {
            if(_isValid)
                TryBuildMachine(building);
        }

        private void TryBuildMachine(Building building)
        {
            if (ToolState.Inputs.UsePressed)
            {
                if (Time.realtimeSinceStartup - _timeBuildTime > buildRate)
                {
                    if (BuildMachine(building))
                    {
                        _timeBuildTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        private bool BuildMachine(Building building)
        {
            if(_selectedMachine == null)
                return false;
            
            var buildCell = AimHandler.HoveredPosition.Value;
            var instance = _selectedMachine.Build(buildCell, building);
            return instance != null;
        }
    }
}