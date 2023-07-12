using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities;
using Zenject;
using Object = UnityEngine.Object;

namespace Items
{
    public class GlobalInventoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ToolLoader>().AsSingle().NonLazy();
            Container.BindFactory<Transform, ToolManager, ToolManager.Factory>().AsSingle().NonLazy();
        }
    }

    public class ToolManager
    {
        private readonly ToolLoader _loader;
        private readonly Transform _parent;
        private readonly Dictionary<Tool, GameObject> _controllerInstances = new();

        public ToolManager(ToolLoader loader, Transform parent)
        {
            _loader = loader;
            _parent = parent;
        }
        public class Factory : PlaceholderFactory<Transform, ToolManager> { }

        public async UniTask<GameObject> GetController(Tool tool)
        {
            if (!_controllerInstances.ContainsKey(tool) || _controllerInstances[tool] == null)
            {
                _controllerInstances.Remove(tool);
                var controllerPrefab = await _loader.GetPrefab(tool);
                var controller = Object.Instantiate(controllerPrefab, _parent, false);
                _controllerInstances.Add(tool, controller);
            }
            return _controllerInstances[tool];
        }
    }
    
    public class ToolLoader : IDisposable
    {

        private Dictionary<Tool, GameObject> _controllerPrefabs = new();

        public ToolLoader()
        {
            
        }
        private static async UniTask<GameObject> GetPrefabFromTool(Tool tool)
        {
            if (tool == null)
                return null;
            return await tool.GetControllerPrefab();
        }
        public async UniTask<GameObject> GetPrefab(Tool tool)
        {
            if (tool == null) return null;
            if (!_controllerPrefabs.ContainsKey(tool) || _controllerPrefabs[tool] == null)
            {
                _controllerPrefabs.Remove(tool);
                var controllerPrefab = await tool.GetControllerPrefab();
                _controllerPrefabs.Add(tool, controllerPrefab);
            }
            return _controllerPrefabs[tool];
        }
        
        [Obsolete("Use Manager instead")]
        public async UniTask<GameObject> GetInstance(Tool tool, Transform parent)
        {
            if (!_controllerPrefabs.ContainsKey(tool) || _controllerPrefabs[tool] == null)
            {
                _controllerPrefabs.Remove(tool);
                var controllerPrefab = await GetPrefab(tool);
                var controller = Object.Instantiate(controllerPrefab, parent, false);
                _controllerPrefabs.Add(tool, controller);
            }
            return _controllerPrefabs[tool];
        }

        public void Dispose()
        {
            
        }
    }
}