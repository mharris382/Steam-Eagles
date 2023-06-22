using System;
using System.Collections.Generic;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
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

        public ToolBeltToCharacterTools(
            ToolBeltInventory toolBeltInventory,
            CharacterTools tools, 
            EntityInitializer entityInitializer,
            CoroutineCaller coroutineCaller)
        {
            _toolBeltInventory = toolBeltInventory;
            _tools = tools;
            _entityInitializer = entityInitializer;
            _coroutineCaller = coroutineCaller;
        }

        public void Initialize()
        {
            _cd = new CompositeDisposable();
            _coroutine = _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                await UniTask.WaitUntil(() => _entityInitializer.isDoneInitializing);
                int cnt = 0;
                foreach (var tool in _toolBeltInventory.GetTools())
                {
                    Debug.Log($"Setting tool {tool} to slot {cnt}");
                    var cnt1 = cnt;
                    tool.Select(t => (t, cnt1)).Subscribe(t => SetTool(t.Item1, t.Item2)).AddTo(_cd);
                    cnt++;
                }
            }));
        }

        void SetTool(Tool tool, int slot)
        {
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                await UniTask.WaitUntil(() => _entityInitializer.isDoneInitializing);
                await _tools.SetTool(tool, slot);
            }));
        }
        
        public void Dispose()
        {
            if (_coroutineCaller != null && _coroutine != null)
            {
                _coroutineCaller.StopCoroutine(_coroutine);
            }
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
        private Dictionary<Tool, GameObject> _controllers = new Dictionary<Tool, GameObject>();

        private ReactiveProperty<LinkedListNode<Tool>> _currentTool = new();

        private Subject<GameObject> _onToolControllerInstantiated = new();
        private ToolSwapController _toolSwapper;

        public IObservable<GameObject> OnToolControllerInstantiated => _onToolControllerInstantiated;

        public async UniTask SetTool(Tool tool, int slot)
        {
            _canSwitchTools.Value = false;
            
            if (_currentToolIndex == slot)
            {
                var prevTool = _tools[slot];
                
                if (prevTool != null && _controllers.ContainsKey(prevTool) && _controllers[prevTool] != null)
                {
                    var prevControllerBase = _controllers[prevTool].gameObject.GetComponent<ToolControllerBase>();
                    _toolSwapper.RemoveTool(prevControllerBase);
                    prevControllerBase.SetActive(false);
                }
            }

            if (!_controllers.TryGetValue(tool, out var controller) || controller == null)
            {
                
                var controllerPrefab =await tool.GetControllerPrefab();
                controller = Instantiate(controllerPrefab, toolParent);
                _onToolControllerInstantiated.OnNext(controller);
                if(_controllers.ContainsKey(tool))_controllers.Remove(tool);
                Debug.Assert(_controllers.ContainsKey(tool) == false, "_controllers.ContainsKey(tool) == false", controller);
                _controllers.Add(tool, controller.gameObject);
            }
            controller.SetActive(true);
            var controllerBase = controller.GetComponent<ToolControllerBase>();
            Debug.Assert(controllerBase != null, "controllerBase != null", controller);
            controllerBase?.SetActive(true);
            _tools[slot] = tool;
            _toolSwapper.AddTool(controllerBase);
            _canSwitchTools.Value = true;
        }

        private void Awake()
        {
            _tools = new Tool[maxToolSlots];
            _toolSwapper = GetComponent<ToolSwapController>();
            
            Tool lastTool = null;
            
            
        }

        
        
    }
}