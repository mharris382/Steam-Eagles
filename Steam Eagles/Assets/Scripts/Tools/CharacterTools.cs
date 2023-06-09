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
            _coroutine = _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                await UniTask.WaitUntil(() => _entityInitializer.isDoneInitializing);
                int cnt = 0;
                foreach (var tool in _toolBeltInventory.GetTools())
                {
                    Debug.Log($"Setting tool {tool} to slot {cnt}");
                    await _tools.SetTool(tool, cnt);
                    cnt++;
                }
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

        public IObservable<GameObject> OnToolControllerInstantiated => _onToolControllerInstantiated;
        private LinkedListNode<Tool> currentTool
        {
            get => _currentTool.Value;
            set => _currentTool.Value = value;
        }
        private LinkedList<Tool> _usableTools = new LinkedList<Tool>();

        private ReadOnlyReactiveProperty<GameObject> _currentToolController;
        public IObservable<GameObject> OnToolEquipped => _currentToolController;

        public async UniTask SetTool(Tool tool, int slot)
        {
            _canSwitchTools.Value = false;
            
            if (_currentToolIndex == slot)
            {
                var prevTool = _tools[slot];
                
                if (prevTool != null && _controllers.ContainsKey(prevTool))
                {
                    var prevControllerBase = _controllers[prevTool].gameObject.GetComponent<ToolControllerBase>();
                    prevControllerBase.SetActive(false);
                    _controllers[prevTool].gameObject.SetActive(false);
                }
            }

            if (!_controllers.TryGetValue(tool, out var controller))
            {
                var controllerPrefab =await tool.GetControllerPrefab();
                controller = Instantiate(controllerPrefab, toolParent);
                _onToolControllerInstantiated.OnNext(controller);
                _controllers.Add(tool, controller.GetComponent<GameObject>());
            }
            controller.SetActive(true);
            var controllerBase = controller.GetComponent<ToolControllerBase>();
            Debug.Assert(controllerBase != null, "controllerBase != null", controller);
            controllerBase?.SetActive(true);
            _tools[slot] = tool;
            _canSwitchTools.Value = true;
        }

        private void Awake()
        {
            _tools = new Tool[maxToolSlots];

            Tool lastTool = null;
            
            _currentToolController = _currentTool.Where(t => t != null).Select(t => t.Value).Select(t => _controllers[t]?.gameObject)
                .Merge(_canSwitchTools.Where(t => !t).Select(_ => (GameObject)null)).ToReadOnlyReactiveProperty();
            _canSwitchTools.Where(t => !t).Subscribe(_ => lastTool = currentTool?.Value).AddTo(this);
            _canSwitchTools.Where(t => t).Subscribe(_ =>
            {
                _usableTools.Clear();
                foreach (var tool in _tools)
                {
                    if (tool != null)
                    {
                        _usableTools.AddLast(tool);
                    }
                }
                if (lastTool != null && _usableTools.Contains(lastTool))
                {
                    currentTool = _usableTools.Find(lastTool);
                }
                else if(_usableTools.Count > 0)
                {
                    currentTool = _usableTools.First;
                }
                else
                {
                    currentTool = null;
                }
            });
            
        }

        
        
    }
}