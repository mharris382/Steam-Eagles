using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using Items;
using Sirenix.OdinInspector;
using Tools.GenericTools;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace Tools.BuildTool
{
    public class ToolBeltToCharacterTools : IInitializable, IDisposable
    {
        private readonly ToolBeltInventory _toolBeltInventory;
        private readonly CharacterTools _tools;
        private readonly EntityInitializer _entityInitializer;
        private readonly CoroutineCaller _coroutineCaller;
        
        private Coroutine _coroutine;
        private CompositeDisposable _cd;
        private ToolSlot[] _toolSlots;
        private Tool[] _toolsArray;
        private Coroutine[] _coroutines;
        
        private readonly ToolSwapController _toolSwapper;
        private readonly ToolControllerLoader _loader;
        protected bool _disposed;

        public ToolBeltToCharacterTools(
            ToolBeltInventory toolBeltInventory,
            CharacterTools tools, 
            EntityInitializer entityInitializer,
            Transform toolParent,
            CoroutineCaller coroutineCaller)
        {
            _toolBeltInventory = toolBeltInventory;
            _tools = tools;
            _entityInitializer = entityInitializer;
            _toolSwapper = _entityInitializer.GetComponent<ToolSwapController>();
            _coroutineCaller = coroutineCaller;
            _loader = new ToolControllerLoader(toolParent);
        }

        public void Initialize()
        {
            _cd = new CompositeDisposable();
            _disposed = false;
            _coroutine = _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                await UniTask.WaitUntil(() => _entityInitializer.isDoneInitializing);
                int cnt = 0;
                _toolSlots = _toolBeltInventory.GetToolSlots().ToArray();
                _toolsArray = _toolSlots.Select(x => x.Tool).ToArray();
                while (true)
                {
                    await UniTask.DelayFrame(4);
                    
                }
            }));
        }

   
        public void Dispose()
        {
            _disposed = true;
            if (_coroutineCaller != null && _coroutine != null)
            {
                _coroutineCaller.StopCoroutine(_coroutine);
            }
            _cd?.Dispose();
        }
    }
    
    public class CharacterTools : MonoBehaviour
    {
        [DisableInPlayMode]
        public int maxToolSlots = 7;
        [Required]
        public Transform toolParent;
        private int _currentToolIndex;
        
        private BoolReactiveProperty _canSwitchTools = new();
        
         private Tool[] _tools;

         private ToolSwapController _toolSwapper;
        private ToolControllerLoader _toolLoader;

        private void Awake()
        {
            _tools = new Tool[maxToolSlots];
            _toolSwapper = GetComponent<ToolSwapController>();
            _toolLoader = new ToolControllerLoader(toolParent);
            Tool lastTool = null;
        }

        private async UniTask<ToolControllerBase> GetController(Tool tool)
        {
            var prevControllerBaseGo = await _toolLoader.GetController(tool);
            if(prevControllerBaseGo == null) throw new NullReferenceException($"prevControllerBaseGo == null, prevTool: {tool}");
            var controller =  prevControllerBaseGo.GetComponent<ToolControllerBase>();
            if (controller == null)
            {
                Debug.LogError($"Tool controller on {tool.itemName} null", tool);
                controller = prevControllerBaseGo.AddComponent<NullTool>();
            }
            return controller;
        }

        public async UniTask SetTool(Tool tool, int slot)
        {
            _canSwitchTools.Value = false;
            
            Debug.Assert(slot >= 0 && slot < maxToolSlots, $"slot >= 0 && slot < maxToolSlots, slot: {slot}, maxToolSlots: {maxToolSlots}");
            
            var prevTool = _tools[slot];
            if (prevTool != null && _toolLoader.HasToolLoaded(prevTool))
            {
                var prevControllerBase = await GetController(prevTool);
                _toolSwapper.RemoveTool(prevControllerBase);
                prevControllerBase.SetActive(false);
            }
            
            if(tool == null)
            {
                _tools[slot] = null;
                _canSwitchTools.Value = true;
                return;
            }
            var controller = await GetController(tool);
            controller.gameObject.SetActive(true);
            _tools[slot] = tool;
            _toolSwapper.AddTool(controller);
            _canSwitchTools.Value = true;
        }
        
      
    }
    public class ToolControllerLoader
    {
        private readonly Transform _toolParent;
        private readonly Dictionary<Tool, GameObject> _loadedControllers = new();
        

        public ToolControllerLoader(Transform toolParent)
        {
            
            _toolParent = toolParent;
        }

        public async UniTask<GameObject> GetController(Tool tool)
        {
            if (tool == null) throw new NullReferenceException();
            if (_loadedControllers.TryGetValue(tool, out var controller) && controller != null)
                return controller;
            _loadedControllers.Remove(tool);
            var inst = await ToolPrefabHelper.CreateInstance(tool, _toolParent);
            Debug.Assert(inst != null, $"Null Prefab for tool: {tool.itemName}", tool);
            _loadedControllers.Add(tool, inst);
            return inst;
        }

        public bool HasToolLoaded(Tool tool)
        {
            return _loadedControllers.ContainsKey(tool) && _loadedControllers[tool] != null;
        }

        
    }
    public static class ToolPrefabHelper
    {
        public static async UniTask<GameObject> GetPrefab(Tool tool)
        {
            if (tool == null)
                return null;
            return await tool.GetControllerPrefab();
        }

        public static async UniTask<GameObject> CreateInstance(Tool tool, Transform parent)
        {
            var go = await GetPrefab(tool);
            if (go == null)
                return null;
            var inst = GameObject.Instantiate(go, parent);
            return inst;
        }
    }

}