using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Characters
{
    public enum ToolStates
    {
        None,
        Recipe,
        Build,
        Destruct,
        Repair,
        
    }
    public class ToolState : MonoBehaviour
    {
        public ToolStates currentToolState; 
        public void SetToolState(ToolStates toolState)
        {
            currentToolState = toolState;
        }
    }
}