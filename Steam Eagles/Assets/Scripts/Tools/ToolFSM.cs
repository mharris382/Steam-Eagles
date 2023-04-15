using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Cysharp.Threading.Tasks;
using Items;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using Tools.BuildTool;
using Tools.DestructTool;
using UniRx;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools
{
    public class ToolFSM : SerializedMonoBehaviour
    {
        private CharacterState _character;

        public Dictionary<ToolStates, ToolControllerBase> toolControllers = new Dictionary<ToolStates, ToolControllerBase>();
        public ToolSlots toolSlots;
        public ToolBeltInventory toolBeltInventory;
        private ToolBelt _toolbelt;
        [Required]
        public Transform toolControllerParent;
        
        private Dictionary<Tool, Coroutine> _loaders = new Dictionary<Tool, Coroutine>();
        private Dictionary<Tool, ToolControllerBase> _controllers = new Dictionary<Tool, ToolControllerBase>();
        private void Awake()
        {
            _character = GetComponent<CharacterState>();
            _toolbelt = new ToolBelt(_character, build:toolBeltInventory.buildToolSlot.Tool, destruct:toolBeltInventory.destructToolSlot.Tool, craft:toolBeltInventory.craftToolSlot.Tool, repair:toolBeltInventory.repairToolSlot.Tool);
            CurrentTool = _toolbelt.CurrentTool;
            CurrentTool.Subscribe(t =>
            {
                if (_controllers.ContainsKey(t))
                {
                    _controllers[t].Activate(t);
                    return;
                }
                if (_loaders.ContainsKey(t))
                {
                    return;
                }
                _loaders.Add(t, StartCoroutine(LoadTool(t)));
                Debug.Log($"Current tool is now: {(t == null ? "null" : t.name)}");
                toolControllers[t.toolState].Activate(t);
            });
        }

        IEnumerator LoadTool(Tool t)
        {
            Debug.Log($"{name} now loading controller for tool: {t.name}",this);
            yield return UniTask.ToCoroutine(async () =>
            {
                var controller = await ToolLoader.Instance.LoadController(t);
                var newController = Instantiate(controller, toolControllerParent);
                _controllers.Add(t, newController.GetComponent<ToolControllerBase>());
                Debug.Assert(_controllers[t] != null, $"Failed to load controller for tool: {t.name}", this);
                
                //if tool is still selected then activate it now that it has loaded, otherwise wait for it to be selected
                if (_toolbelt.CurrentTool.Value == t)
                {
                    _controllers[t].Activate(t);
                }
            });
        }
        
        
        public ReadOnlyReactiveProperty<Tool> CurrentTool { get; set; }
    }



    public class ToolBelt
    {
        private const int BUILD_TOOL_SLOT = 0;
        private const int DESTRUCT_TOOL_SLOT = 1;
        private const int CRAFT_TOOL_SLOT = 2;
        private const int REPAIR_TOOL_SLOT = 3;

        private readonly ToolBeltSlot[] _slots = new ToolBeltSlot[4];
        private readonly ReadOnlyReactiveProperty<Tool> _currentTool;

        public ReadOnlyReactiveProperty<Tool> CurrentTool => _currentTool;

        public ToolBelt(CharacterState characterState, 
            Tool repair =null, 
            Tool craft = null, 
            Tool build = null, 
            Tool destruct = null)
        {
            _slots[BUILD_TOOL_SLOT] = new ToolBeltSlot(characterState,ToolStates.Build);
            _slots[DESTRUCT_TOOL_SLOT] = new ToolBeltSlot(characterState,ToolStates.Destruct);
            _slots[CRAFT_TOOL_SLOT] = new ToolBeltSlot(characterState,ToolStates.Recipe);
            _slots[REPAIR_TOOL_SLOT] = new ToolBeltSlot(characterState,ToolStates.Repair);
            
            _currentTool = _slots[0].OnToolBecameActive
                .Where(t => t.Item2)
                .Select(t => t.Item1)
                .Merge(_slots[1].OnToolBecameActive
                    .Where(t => t.Item2)
                    .Select(t => t.Item1))
                .Merge(_slots[2].OnToolBecameActive
                    .Where(t => t.Item2)
                    .Select(t => t.Item1))
                .Merge(_slots[3].OnToolBecameActive
                    .Where(t => t.Item2)
                    .Select(t => t.Item1)).ToReadOnlyReactiveProperty();
        }
        
        
    }

    public class ToolBeltSlot : IDisposable
    {
        private readonly ToolStates _state;

        public Tool Tool
        {
            get => _tool.Value;
            set => _tool.Value = value;
        }

        private readonly ReactiveProperty<Tool> _tool = new ReactiveProperty<Tool>();
        private readonly ReactiveProperty<bool> _active = new ReactiveProperty<bool>();
        
        public IObservable<(Tool, bool)> OnToolBecameActive => _tool.CombineLatest(_active, (t, a) => (t, a));
        
        private IDisposable _disposable;
            
            

        public ToolBeltSlot(CharacterState characterState, ToolStates state)
        {
            this._state = state;
            _disposable = characterState.Tool.toolState.Subscribe(t => _active.Value = state == t);
        }

        public void Dispose()
        {
            _active.Dispose();
            _disposable?.Dispose();
            _disposable = null;
        }
    }
}