using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using CoreLib.Interfaces;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class ToolSwitchboard : SmartSwitchboard<ToolControllerBase>
    {
        private readonly ToolControllerSharedData _toolData;
        private readonly ToolState _toolState;


        public ToolSwitchboard(ToolControllerSharedData toolData, ToolState toolState)
        {
            _toolData = toolData;
            _toolState = toolState;
        }
        protected override bool IsValid(int index, ToolControllerBase value) => value.tool != null;

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


    public class ToolSwapController : MonoBehaviour, IHideTools
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