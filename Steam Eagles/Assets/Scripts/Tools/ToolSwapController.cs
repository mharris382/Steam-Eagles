using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using CoreLib;
using CoreLib.Interfaces;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    // public class ToolSlotSwitchboard : SmartSwitchboard<ToolSlot>
    // {
    //     private readonly ToolControllerSharedData _toolData;
    //     private readonly ToolState _toolState;
    //     private readonly ToolControllerLoader _toolLoader;
    //     private readonly Dictionary<Tool, Coroutine> _loadingToolControllers = new Dictionary<Tool, Coroutine>();
    //     private bool _isLoadingToolController = false;
    //
    //     public ToolSlotSwitchboard(ToolControllerSharedData toolData, ToolState toolState, Transform toolParent)
    //     {
    //         _toolData = toolData;
    //         _toolState = toolState;
    //         _toolLoader = new ToolControllerLoader(toolParent);
    //     }
    //
    //     public override bool Add(ToolSlot value)
    //     {
    //         var result =  base.Add(value);
    //         
    //         return result;
    //     }
    //
    //     protected override bool IsValid(int index, ToolSlot value)
    //     {
    //         if (value == null)
    //             return false;
    //         
    //         if (value.Tool == null) 
    //             return false;
    //         
    //         StartLoadingToolController(value);
    //         if (!value.Controller) 
    //             return false;
    //
    //         var controller = value.Controller.GetComponent<ToolControllerBase>();
    //         
    //         if(controller == null)
    //         {
    //             Debug.LogError($"Controller is null for tool: {value.Tool.itemName}", value);
    //             return false;
    //         }
    //         
    //         if (controller.Building == null && !controller.CanBeUsedOutsideBuilding())
    //             return false;
    //         
    //         return true;
    //     }
    //
    //     private void StartLoadingToolController(ToolSlot toolSlot, Action<bool> onComplete = null)
    //     {
    //         if (toolSlot == null) return;
    //         
    //         Tool tool = toolSlot.Tool;
    //         if(tool == null)return;
    //
    //         if (_loadingToolControllers.ContainsKey(tool)) return;
    //         if (_isLoadingToolController) return;
    //         _isLoadingToolController = true;
    //        _loadingToolControllers.Add(tool, _toolData.StartCoroutine(UniTask.ToCoroutine(async () =>
    //         {
    //             try
    //             {
    //                 var controller = await _toolLoader.GetController(tool);
    //                 toolSlot.Controller = controller;
    //                 _isLoadingToolController = false;
    //                 onComplete?.Invoke(true);
    //             }
    //             catch (Exception e)
    //             {
    //                 Debug.LogError($"{tool.itemName} failed to load: {e.Message}");
    //                 onComplete?.Invoke(false);
    //             }
    //             finally
    //             {
    //                 _isLoadingToolController = false;
    //             }
    //         })));
    //     }
    //
    //     protected override void ValueActivated(ToolSlot value)
    //     {
    //         if (TryGetController(value, out var controller))
    //         {
    //             ValueActivated(controller);
    //         }
    //     }
    //
    //     protected override void ValueDeactivated(ToolSlot value)
    //     {
    //         if (TryGetController(value, out var controller))
    //         {
    //             ValueDeactivated(controller);
    //         }
    //     }
    //
    //     
    //     protected void ValueActivated(ToolControllerBase value)
    //     {
    //         if (value != null)
    //         {
    //             value.gameObject.SetActive(true);
    //             value.SetActive(true);
    //             value.SetPreviewVisible(true);
    //             value.SetToolEquipped(true);
    //             _toolState.toolState.Value = value.tool.toolState;
    //         }
    //         else
    //         {
    //             _toolState.toolState.Value = ToolStates.None;
    //         }
    //         _toolData.activeTool.Value = value;
    //     }
    //
    //     protected  void ValueDeactivated(ToolControllerBase value)
    //     {
    //         if(value!=null)
    //         {
    //             value.gameObject.SetActive(false);
    //             value.SetActive(false);
    //             value.SetToolEquipped(false);
    //             value.SetPreviewVisible(false);
    //         }
    //         
    //     }
    //
    //     private  bool TryGetController(ToolSlot value, out ToolControllerBase controller)
    //     {
    //         controller = null;
    //         if (!_toolLoader.HasToolLoaded(value.Tool))
    //         {
    //             return false;
    //         }
    //         if(value.Controller == null)
    //         {
    //             Debug.LogError($"Controller is null: {value.name}", value);
    //             return false;
    //         }
    //
    //         controller = value.Controller.GetComponent<ToolControllerBase>();
    //         if (controller == null)
    //         {
    //             Debug.LogError($"Controller is null -> slot:{value.name}\t go: {value.Controller}", value.Controller);
    //             return false;
    //         }
    //         
    //         return true;
    //     }
    // }
    //
    // [Obsolete("use Tool Slot Switchboard instead")]
    public class ToolSwitchboard : SmartSwitchboard<ToolControllerBase>
    {
        private readonly ToolControllerSharedData _toolData;
        private readonly ToolState _toolState;


        public ToolSwitchboard(ToolControllerSharedData toolData, ToolState toolState)
        {
            _toolData = toolData;
            _toolState = toolState;
        }
        protected override bool IsValid(int index, ToolControllerBase value)
        {
            if(value == null)return false;
            if (!value.CanBeUsedOutsideBuilding() && value.Building == null)
            {
                return false;
            }
            return value.tool != null;
        }

        protected override void ValueActivated(ToolControllerBase value)
        {
            if (value != null)
            {
                value.gameObject.SetActive(true);
                value.SetActive(true);
                value.SetPreviewVisible(true);
                value.SetToolEquipped(true);
               _toolState.toolState.Value = value.tool.toolState;
            }
            else
            {
               _toolState.toolState.Value = ToolStates.None;
            }
            _toolData.activeTool.Value = value;
        }

        protected override void ValueDeactivated(ToolControllerBase value)
        {
            if(value!=null)
            {
                value.gameObject.SetActive(false);
                value.SetActive(false);
                value.SetToolEquipped(false);
                value.SetPreviewVisible(false);
            }
            
        }
    }


    public class ToolSwapController : MonoBehaviour, IHideTools, IToolControllerSlots
    {
        [SerializeField] private float toolSwitchRate = .2f;
        [SerializeField] private Transform toolContainer;
        private float _lastToolSwapTime;

        private BoolReactiveProperty _toolsHidden = new BoolReactiveProperty(false);

        private ToolSwitchboard _toolSwitchboard;
        private ToolState _toolState;
        private ToolState ToolState => _toolState != null ? _toolState : _toolState = GetComponentInParent<ToolState>();

        

        public bool ToolsHidden
        {
            get => _toolsHidden.Value;
            set => _toolsHidden.Value = value;
        }

        private void Awake()
        {
            var toolData = this.GetComponent<ToolControllerSharedData>();
            _toolSwitchboard = new ToolSwitchboard(toolData, this.GetComponent<ToolState>());
            var containerGo = toolContainer.gameObject;
            _toolsHidden.Select(t => !t).Subscribe(containerGo.SetActive).AddTo(this);
            _toolsHidden.Select(t => (t, _toolSwitchboard.Current)).Where(t => t.Current != null).Subscribe(t =>
            {
                var value = t.Current;
                value.gameObject.SetActive(t.t);
                value.SetActive(t.t);
                value.SetToolEquipped(t.t);
                value.SetPreviewVisible(t.t);
            }).AddTo(this);
            
            //foreach (var tool in tools) _toolSwitchboard.Add(tool);
            _toolsHidden.Subscribe(t => toolData.ToolsHidden = t).AddTo(this);
            // _toolsHidden.Subscribe(AllTools.SetHidden).AddTo(this);
            StartCoroutine(SlowTick());
        }

        IEnumerator SlowTick()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(0.125f);
                _toolSwitchboard.SlowTick(0.125f);
            }
        }
        public void AddTool(GameObject go)
        {
            var controller = go.GetComponent<ToolControllerBase>();
            AddTool(controller);
        }

        public void RemoveTool(GameObject go)
        {
            var controller = go.GetComponent<ToolControllerBase>();
            _toolSwitchboard.Remove(controller);
        }
        public void AddTool(ToolControllerBase controllerBase)
        {
            controllerBase.gameObject.SetActive(false);
            _toolSwitchboard.Add(controllerBase);
        }

        public void RemoveTool(ToolControllerBase controllerBase)
        {
            _toolSwitchboard.Remove(controllerBase);
        }

        private void Update()
        {
            if(ToolsHidden) return;
            if (Time.realtimeSinceStartup - _lastToolSwapTime > toolSwitchRate
                && ToolState.Inputs.SelectTool != 0)
            {
                _lastToolSwapTime = Time.realtimeSinceStartup;
                if (ToolState.Inputs.SelectTool > 0)
                {
                    _toolSwitchboard.Next();
                    //AllTools.Next();
                }
                else
                {
                    _toolSwitchboard.Prev();
                }
            }
        }


        private void OnDestroy()
        {
            
        }
    }
}