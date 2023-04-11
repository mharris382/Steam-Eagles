using CoreLib;
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



        private ToolInputs _inputs = new ToolInputs();
        public ToolInputs Inputs => _inputs;
        
        public class ToolInputs
        {
            /// <summary>
            /// raw input from the player device
            /// </summary>
            public Vector2 AimInput
            {
                get;
                set;
            }    
            
            public InputMode CurrentInputMode
            {
                get;
                set;
            }
            
        }
    }
}