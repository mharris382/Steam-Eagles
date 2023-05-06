using Tools.BuildTool;
using UnityEngine;

namespace Players.PCController.Tools
{
    public class PCToolSystems 
    {
        
    }
    
    public class PCToolSystem : PCSystem
    {
        private ToolSwapController _toolSwapController;
        public PCToolSystem(PC pc) : base(pc) => _toolSwapController = pc.PCInstance.character.GetComponent<ToolSwapController>();
    }
}