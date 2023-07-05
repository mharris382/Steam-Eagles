using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Interfaces;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Items
{
    [RequireComponent(typeof(InventorySlot))]
    public class ToolSlot : MonoBehaviour
    {
        public Tool tool;
        public bool IsEmpty => tool == null;
        
        private ReactiveProperty<Tool> _tool = new();
        private InventorySlot _slot;
        private IToolControllerSlots _toolControllerSlots;
        private DynamicReactiveProperty<GameObject> _controllerInstance = new();
        private Subject<(Tool, GameObject)> _onToolChanged = new();

        [InjectOptional] private Transform _toolParent;
        private ToolManager _prefabLoader;

        [ShowInInspector, ReadOnly] public Tool Tool
        {
            get => _tool.Value;
            set => _tool.Value = value;
        }

        [ShowInInspector, ReadOnly] public GameObject Controller
        {
            get => _controllerInstance.Value;
            set => _controllerInstance.Set(value);
        }

        public IObservable<Tool> OnToolChanged
        {
            get
            {
                if(_tool.HasValue)
                    return _tool.StartWith(_tool.Value);
                else
                {
                    return _tool;
                }
            }
        }


        public Transform ControllerParent => _toolParent ? _toolParent : transform;

        public IObservable<GameObject> ControllerUnEquippedFromSlot => _controllerInstance.OnSwitched.Select(t => t.previous);
        public IObservable<GameObject> ControllerEquippedToSlot =>  _controllerInstance.OnSwitched.Select(t => t.next);
        
        
        public void InjectMe(ToolManager loader)
        {
            _prefabLoader = loader;
        }

        private void Awake()
        {
            _slot = GetComponent<InventorySlot>();
            _toolControllerSlots = GetComponentInParent<IToolControllerSlots>();
            Debug.Assert(_toolControllerSlots != null, "missing IToolControllerSlots", this);
            var tool = _slot.ItemStack.item as Tool;
            _tool = new ReactiveProperty<Tool>(tool);
        }

        void SetTool(Tool t)
        {
            return;
            if (t == null)
            {
                _onToolChanged.OnNext((t, null));
                return;
            }
            StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var controllerGo = await _prefabLoader.GetController(tool);
                _onToolChanged.OnNext((t, controllerGo));
            }));
        }
        private IEnumerator Start()
        {
            yield break;
            while (_slot.IsSlotLoading)
            {
                yield return null;
            }
            
            _slot.onStackChanged.AsObservable().StartWith(_slot.ItemStack).Select(t => t.Item as Tool).Subscribe(SetTool).AddTo(this);
            _onToolChanged.Subscribe(t =>
            {
                _controllerInstance.Set(t.Item2);
                _tool.Value = t.Item1;
            }).AddTo(this);
            _controllerInstance.OnSwitched.Subscribe(t =>
            {
                if (t.previous != null) _toolControllerSlots.RemoveTool(t.previous);
                if (t.next != null) _toolControllerSlots.AddTool(t.next);
            }).AddTo(this);
            
        }
    }
}