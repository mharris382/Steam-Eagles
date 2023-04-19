using CoreLib;
using UniRx;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace CoreLib
{
}

namespace Characters
{
    [System.Serializable]
    public class ToolStateReactiveProperty : ReactiveProperty<ToolStates> { }

    public class ToolState : MonoBehaviour
    {
        public float maxToolRange = 5f;

        public ToolStates currentToolState
        {
            get => toolState.Value;
            set => toolState.Value = value;
        }

        public void SetToolState(ToolStates toolState)
        {
            currentToolState = toolState;
        }

        public ToolStateReactiveProperty toolState = new ToolStateReactiveProperty();
        


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
            
            
            public int CurrentToolIndex
            {
                get;
                set;
            }
            
            public int AvailableNumberOfTools
            {
                get;
                set;
            }

            public int SelectTool
            {
                get;
                set;
            }
        }
    }
}