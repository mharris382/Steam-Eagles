using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    
    [Serializable]
    public class ReactiveToolController : ReactiveProperty<ToolControllerBase>
    {
    
    }
    public class ToolControllerSharedData : MonoBehaviour
    {
        public ReactiveToolController activeTool;
        public ReactiveProperty<ToolControllerBase> ActiveTool => activeTool;
        private IntReactiveProperty _currentToolIndex = new IntReactiveProperty(0);
        private StringReactiveProperty _errorMessage = new StringReactiveProperty();
        private BoolReactiveProperty _toolsHidden = new();
        public List<ToolControllerBase> tools = new List<ToolControllerBase>();
        public List<Tool> toolItems = new List<Tool>();

        private ReactiveProperty<bool> _toolsEquipped = new ReactiveProperty<bool>(true);

        public ToolControllerBase ActiveToolValue => ActiveTool.Value;


        public bool ToolsHidden
        {
            get => _toolsHidden.Value;
            set => _toolsHidden.Value = value;
        }

        public int AvailableTools => tools.Count;

        public IObservable<bool> ToolsHiddenStream => _toolsHidden.StartWith(_toolsHidden.Value);

        public StringReactiveProperty ErrorMessage => _errorMessage;


        public int CurrentToolIndex
        {
            get => _currentToolIndex.Value;
            set => _currentToolIndex.Value = value;
        }

        public bool ToolsEquipped
        {
            get=> _toolsEquipped.Value;
            set => _toolsEquipped.Value = value;
        }


        public void NextTool()
        {
            if (tools.Count == 0)
                return;
            CurrentToolIndex = (_currentToolIndex.Value + 1) % tools.Count;
            activeTool.Value = tools[_currentToolIndex.Value];
            Debug.Log($"Current Tool: {activeTool.Value}");
        }

        public void PrevTool()
        {
            if (tools.Count == 0)
            {
                return;
            }
            CurrentToolIndex = (_currentToolIndex.Value - 1) % tools.Count;
            activeTool.Value = tools[_currentToolIndex.Value];
            Debug.Log($"Current Tool: {activeTool.Value}");
        }

        public IReadOnlyReactiveProperty<bool> ToolsEquippedProperty => _toolsEquipped;
        private void Awake()
        {
            if(activeTool == null)
                activeTool = new ReactiveToolController();
            activeTool.Subscribe(t =>
            {
                foreach (var tool in tools)
                {
                    tool.gameObject.SetActive(tool == t);
                }
            }).AddTo(this);
            _currentToolIndex.Subscribe(index =>
            {
                Debug.Log($"Current Tool Index: {index}",this);
                index = Mathf.Clamp(index, 0, tools.Count - 1);
                if (tools.Count == 0)
                {

                    return;
                }

                var tool = tools[index];
                activeTool.Value = tool;
            }).AddTo(this);
            ToolsEquipped = true;
        }
        
        public void RegisterTool(ToolControllerBase tool)
        {
            if (activeTool == null)
            {
                activeTool = new ReactiveToolController();
            }
            if(activeTool.Value == null)
                activeTool.Value = tool;
            
            if(!tools.Contains(tool))
            {
                tools.Add(tool);
            }
            NextTool();
            PrevTool();
        }
        
        public void UnregisterTool(ToolControllerBase tool)
        {
            if (activeTool.Value == tool)
            {
                activeTool.Value = tools.FirstOrDefault();
            }
            tools.Remove(tool);
        }

        public void AddTool(Tool tool)
        {
            if (toolItems != null)
            {
                toolItems.Add(tool);
            }
        }

        public void UpdateTool()
        {
            
        }
    }
}