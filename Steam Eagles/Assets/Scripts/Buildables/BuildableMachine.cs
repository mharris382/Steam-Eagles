using System;
using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables
{
    public class BuildableMachine : BuildableMachineBase
    {
       
        [SerializeField]
        private Vector2Int cellSize = Vector2Int.one * new Vector2Int(3, 2);
        
        [SerializeField,TableList]
        private List<MachineCell> machineCells = new List<MachineCell>();
        
        [System.Serializable]
        public class MachineCell
        {
            [SerializeField]
            [Required] public TileBase tile;
            public Vector2Int cellPosition;
            public BuildingLayers targetLayer;
            
            public Color gizmoColor = Color.green;
        }

        public Vector2Int CellSize => cellSize;

        public override Vector2Int MachineGridSize => cellSize;
        

        public Vector2 WsSize
        {
            get
            {
                if (GridLayout == null)
                {
                    return this.cellSize;
                }
                return (Vector2)GridLayout.cellSize * cellSize;
            }
        }

        
    }
}
