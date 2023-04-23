using System;
using System.Collections.Generic;
using Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class ToolSwapController : MonoBehaviour
    {
        [SerializeField] private float toolSwitchRate = .2f;
        [SerializeField] private Transform toolContainer;
        private float _lastToolSwapTime;


        private ToolState _toolState;
        private ToolState ToolState => _toolState != null ? _toolState : _toolState = GetComponentInParent<ToolState>();

        private ToolList _toolList;
        public ToolList AllTools => _toolList ??= new ToolList(this); 
        
        public class ToolList : IDisposable
        {
            private List<ToolControllerBase> _controllers;
            private readonly ToolControllerSharedData _sharedData;
            private ReactiveProperty<int> _activeTool;
            private readonly CompositeDisposable cd;

            private ReactiveProperty<ToolControllerBase> _activeToolController;
            public IReadOnlyReactiveProperty<ToolControllerBase> ActiveToolController => _activeToolController;
            public ToolList(ToolSwapController controller)
            {
                cd = new CompositeDisposable();
                this._sharedData = controller.GetComponent<ToolControllerSharedData>();
                _controllers = new List<ToolControllerBase>(controller.GetComponentsInChildren<ToolControllerBase>());
                _activeTool = new ReactiveProperty<int>();
                _activeToolController = new ReactiveProperty<ToolControllerBase>(_controllers[0]);
                for (int i = 0; i < controller.toolContainer.childCount; i++)
                {
                    var child = controller.toolContainer.GetChild(i).GetComponent<ToolControllerBase>();
                    if (child != null && !_controllers.Contains(child))
                    {
                        _controllers.Add(child);
                    }
                }

                Debug.Assert(_controllers.Count > 0, "_controllers.Count > 0");
                
                
                if (_controllers.Contains(_sharedData.ActiveToolValue))
                {
                    _activeTool.Value = _controllers.IndexOf(_sharedData.ActiveToolValue);
                }

                _activeTool.Select(t => _controllers[t % _controllers.Count])
                    .Subscribe(x =>
                    {
                        _sharedData.activeTool.Value = x;
                        
                        var prevTool = _activeToolController.Value;
                        if (prevTool != null)
                        {
                            prevTool.gameObject.SetActive(false);
                            prevTool.OnToolUnEquipped();
                        }
                        
                        _activeToolController.Value = x;
                        
                        if(x != null)
                        {
                            x.gameObject.SetActive(true);
                            x.OnToolEquipped();
                        }
                        Debug.Log($"Switched tools from {prevTool} to {x}");
                    })
                    .AddTo(cd);

                var toolState = controller.GetComponent<ToolState>();
                Debug.Assert(toolState != null, "toolState != null");
                _activeTool.Select(t => _controllers[t % _controllers.Count].tool)
                    .Subscribe(x => toolState.EquippedTool = x)
                    .AddTo(cd);

                _activeTool.AddTo(cd);
            }

            public void Next()
            {
                var index = _activeTool.Value;
                index++;
                if(index >= _controllers.Count)
                    index = 0;
                _activeTool.Value = index;
                
            }

            public void Prev()
            {
                var index = _activeTool.Value;
                index--;
                if (index < 0)
                    index = _controllers.Count - 1;
                _activeTool.Value = index;
            }

            public void Dispose()
            {
                cd.Dispose();
            }
        }
        
        private void Update()
        {
            if (Time.realtimeSinceStartup - _lastToolSwapTime > toolSwitchRate
                && ToolState.Inputs.SelectTool != 0)
            {
                _lastToolSwapTime = Time.realtimeSinceStartup;
                if (ToolState.Inputs.SelectTool > 0)
                {
                    AllTools.Next();
                }
                else
                {
                    AllTools.Prev();
                }
            }
        }


        private void OnDestroy()
        {
            AllTools.Dispose();
        }
    }
}