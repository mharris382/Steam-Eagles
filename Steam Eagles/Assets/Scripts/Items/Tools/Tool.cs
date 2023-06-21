﻿using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/Items/Tool", order = 0)]
    public class Tool : ItemBase, IIconable, ITool
    {
        public override int MaxStackSize => 1;
        public override bool IsStackable => false;


        [ToggleGroup(nameof(usesRecipes), "Recipes")]
        [SerializeField] private bool usesRecipes;
        [ToggleGroup(nameof(usesRecipes)), InlineEditor(Expanded = true)]
        [SerializeField] private List<Recipe> recipes;

        public override ItemType ItemType => ItemType.TOOL;


        public IEnumerable<Recipe> Recipes => recipes;


        
        [ValidateInput(nameof(ValidateState), "Must assign a valid tool state!")]
        public ToolStates toolState;

        public ToolControllerReference controllerPrefab;
        
        bool ValidateState(ToolStates state) => state != ToolStates.None;

        public Sprite GetIcon() => this.icon;


        public bool UsesRecipes()
        {
            if (usesRecipes)
            {
                this.recipes.Select(t => new CoreRecipe(t, t.components.Select(g => (g.ItemName, g.Count)).ToList()));
                return true;
            }

            return false;
        }


        private GameObject _controllerPrefab;
        private AsyncOperationHandle<GameObject> _controllerPrefabLoadOp;
        private bool _loaded;
        public bool IsControllerPrefabLoaded() => _controllerPrefab != null;

        public async UniTask<GameObject> GetControllerPrefab()
        {
            if (_controllerPrefab == null && !_loaded)
            {
                _loaded = true;
                _controllerPrefabLoadOp =controllerPrefab.LoadAssetAsync<GameObject>();
                _controllerPrefab = await _controllerPrefabLoadOp;
            } else if(_controllerPrefab == null && _loaded)
            {
                if (!_controllerPrefabLoadOp.IsDone)
                {
                    await _controllerPrefabLoadOp;
                }
                _controllerPrefab = _controllerPrefabLoadOp.Result;
            }
            return _controllerPrefab;
        }

    }
}