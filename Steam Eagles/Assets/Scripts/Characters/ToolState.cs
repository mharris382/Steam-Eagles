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
        public float maxToolRange = 5f;
        public void SetToolState(ToolStates toolState)
        {
            currentToolState = toolState;
        }



        private ToolInputs _inputs = new ToolInputs();
        public ToolInputs Inputs => _inputs;
        public float SqrMaxToolRange => maxToolRange * maxToolRange;
        public Vector3 AimPositionLocal
        {
            get;
            set;
        }

        public Vector3 AimPositionWorld
        {
            get => transform.TransformPoint(AimPositionLocal);
            set => AimPositionLocal = transform.InverseTransformPoint(value);
        }
        
        public class ToolInputs
        {
            /// <summary>
            /// raw input from the player device
            /// </summary>
            public Vector2 AimInputRaw
            {
                get;
                set;
            }

          
            
            public InputMode CurrentInputMode
            {
                get;
                set;
            }
            
            public bool UsePressed
            {
                get;
                set;
            }
            
            public bool CancelPressed
            {
                get;
                set;
            }
        }
    }
}