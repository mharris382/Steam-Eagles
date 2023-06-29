using UnityEngine;

namespace CoreLib.Interfaces
{
    public interface IHideTools
    {
        bool ToolsHidden { get; set; }
    }

    public interface IToolControllerSlots
    {
        void AddTool(GameObject controller);
        void RemoveTool(GameObject controller);
    }
}