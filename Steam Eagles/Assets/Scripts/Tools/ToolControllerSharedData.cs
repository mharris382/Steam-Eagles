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
        
        
        public ToolControllerBase ActiveToolValue => ActiveTool.Value;
        
        
        public int AvailableTools => tools.Count;
        private IntReactiveProperty _currentToolIndex = new IntReactiveProperty(0);

        [Button]
        public void NextTool()
        {
            if (tools.Count == 0)
                return;
            _currentToolIndex.Value = (_currentToolIndex.Value + 1) % tools.Count;
            activeTool.Value = tools[_currentToolIndex.Value];
            Debug.Log($"Current Tool: {activeTool.Value}");
        }

        public void PrevTool()
        {
            if (tools.Count == 0)
            {
                return;
            }
            _currentToolIndex.Value = (_currentToolIndex.Value - 1) % tools.Count;
            activeTool.Value = tools[_currentToolIndex.Value];
            Debug.Log($"Current Tool: {activeTool.Value}");
        }
        
        public List<ToolControllerBase> tools = new List<ToolControllerBase>();
        public List<Tool> toolItems = new List<Tool>();
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
    }
}