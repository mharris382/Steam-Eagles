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

        public ToolStateReactiveProperty toolState = new();
        private ToolInputs _inputs = new();
        private ReactiveProperty<ITool> _equippedTool = new();

        public ToolStates currentToolState
        {
            get => toolState.Value;
            set => toolState.Value = value;
        }

        public ITool EquippedTool
        {
            get => _equippedTool.Value;
            set => _equippedTool.Value = value;
        }
        public IReadOnlyReactiveProperty<ITool> EquippedToolRP => _equippedTool;

        public float SqrMaxToolRange => maxToolRange * maxToolRange;

        public ToolInputs Inputs => _inputs;

        public Vector3 AimPositionLocal
        {
            get;
            set;
        }

        public Vector3 AimPositionWorld
        {
            get =>  transform.TransformPoint(AimPositionLocal);
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

            public bool UseHeld
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

            public int SelectRecipe
            {
                get;
                set;
            }

            public Subject<Unit> OnToolModeChanged { get; } = new Subject<Unit>();
        }
    }
}