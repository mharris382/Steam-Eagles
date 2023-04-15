using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Cysharp.Threading.Tasks;
using Items;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tools
{
    public class ToolLoader : Singleton<ToolLoader>
    {
        public override bool DestroyOnLoad => false;


        private Dictionary<Tool, ToolControllerBase> _controllerPrefabLookup = new Dictionary<Tool, ToolControllerBase>();
        
        
        
        public UniTask<ToolControllerBase> LoadController(Tool tool)
        {
            if (_controllerPrefabLookup.TryGetValue(tool, out var controller))
                return UniTask.FromResult(controller);
            
            return Addressables.LoadAssetAsync<ToolControllerBase>(tool.controllerPrefab).ToUniTask().ContinueWith(t =>
            {
                _controllerPrefabLookup.Add(tool, t);
                return t;
            });
        }
        
        public bool TryGetController(Tool tool, out ToolControllerBase controllerBase)
        {
            return _controllerPrefabLookup.TryGetValue(tool, out controllerBase);
        }

        public bool LoadedDefaultTools { get; private set; }
        private IEnumerator Start()
        {
            var toolAddresses = new string[]
            {
                "BuildTool_item",
                "RepairTool_item",
                "DestroyTool_item",
                "RecipeTool_item"
            };
            
            yield return UniTask.ToCoroutine(async () =>
            {
                var tools = await UniTask.WhenAll(toolAddresses.Select(t => Addressables.LoadAssetAsync<Tool>(t).ToUniTask()));
                var controllers = await UniTask.WhenAll(tools.Select(LoadController));
                for (int i = 0; i < tools.Length; i++)
                {
                    _controllerPrefabLookup.Add(tools[i], controllers[i]);
                }

                Debug.Log($"Loaded {tools.Length} default tools", this);
                LoadedDefaultTools = true;
            });
            
        }
    }
}