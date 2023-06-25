using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Items
{
    public class GlobalInventoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<ToolPrefabHelper, ToolPrefabHelper.Factory>().AsSingle().NonLazy();
        }
    }
    public class ToolPrefabHelper : IDisposable
    {
        public class Factory : PlaceholderFactory<ToolPrefabHelper> { }
        Dictionary<Tool, GameObject> _controllers = new();
        
        public static async UniTask<GameObject> GetPrefab(Tool tool)
        {
            if (tool == null)
                return null;
            return await tool.GetControllerPrefab();
        }

        public async UniTask<GameObject> GetInstance(Tool tool, Transform parent)
        {
            if (!_controllers.ContainsKey(tool) || _controllers[tool] == null)
            {
                _controllers.Remove(tool);
                var controllerPrefab = await GetPrefab(tool);
                var controller = Object.Instantiate(controllerPrefab, parent, false);
                _controllers.Add(tool, controller);
            }
            return _controllers[tool];
        }

        public void Dispose()
        {
            foreach (var kvp in _controllers)
            {
                if (kvp.Value != null)
                {
                    Object.Destroy(kvp.Value);
                }
            }
            _controllers.Clear();
        }
    }
}