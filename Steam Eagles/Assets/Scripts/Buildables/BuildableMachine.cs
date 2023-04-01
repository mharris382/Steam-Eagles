using System;
using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables
{
    public class BuildableMachine : MonoBehaviour
    {

        private GridLayout _gridLayout;
        public GameObject buildingTarget;

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
        
        public Vector2Int CellPosition => (Vector2Int) GridLayout.WorldToCell(transform.position);
        
        public Vector2 WsSize => (Vector2)_gridLayout.cellSize * cellSize;

        public GridLayout GridLayout
        {
            get
            {
                if(_gridLayout != null)return _gridLayout;
                if (!HasResources())
                {
                    Debug.LogError("No grid found on building target or building not set",this);
                    return null;
                }
                _gridLayout = FindGridOnTarget();
                return _gridLayout;
            }            
       }
        bool HasResources()
        {
            if(_gridLayout != null)
                return true;
            
            if(buildingTarget == null) 
                return false;

            _gridLayout = FindGridOnTarget();
            return _gridLayout != null;
        }

        GridLayout FindGridOnTarget()
        {
            var grid = buildingTarget.GetComponent<Grid>();
            if (grid == null)
            {
                var tm = buildingTarget.GetComponent<Tilemap>();
                if (tm == null)
                {
                    Debug.LogError($"No grid or tilemap found on building target {buildingTarget.name}",buildingTarget) ;
                    return null;
                }
                _gridLayout = tm.layoutGrid;
            }
            else
            {
                _gridLayout = grid;
            }

            return grid;
        }


        private void OnDrawGizmos()
        {
            if (!HasResources()) return;
            var cellPos = GridLayout.WorldToCell(transform.position);
            var gridCellSize = GridLayout.cellSize;
            Gizmos.color = Color.red;
            Vector3[] GetCorners(Vector3 worldPosition)
            {
               var corners = new Vector3[]
               {
                   new Vector3(worldPosition.x, worldPosition.y, 0),
                   new Vector3(worldPosition.x+gridCellSize.x, worldPosition.y, 0),
                   new Vector3(worldPosition.x+gridCellSize.x, worldPosition.y+gridCellSize.y, 0),
                   new Vector3(worldPosition.x, worldPosition.y+gridCellSize.y, 0),
                   new Vector3(worldPosition.x, worldPosition.y, 0),
               };
                return corners;
            }
            for (int x = 0; x < cellSize.x; x++)
            {
                for (int y = 0; y < cellSize.y; y++)
                {
                    var cell = cellPos + new Vector3Int(x, y, 0);
                    var worldPos = GridLayout.CellToWorld(cell);
                    var corners = GetCorners(worldPos);
                    for (int i = 1; i < corners.Length; i++)
                    {
                        var p0 = corners[i - 1];
                        var p1 = corners[i];
                        Gizmos.DrawLine(p0, p1);
                    }
                }
            }

            foreach (var machineCell in machineCells)
            {
                if(machineCell.tile == null)
                    continue;
                var cell = cellPos + new Vector3Int(machineCell.cellPosition.x, machineCell.cellPosition.y, 0);
                var worldPos = GridLayout.CellToWorld(cell);
                var corners = GetCorners(worldPos);
                Gizmos.color = machineCell.gizmoColor;
                // for (int i = 1; i < corners.Length; i++)                    
                // {                                                           
                //     var p0 = corners[i - 1];                                
                //     var p1 = corners[i];                                    
                //     Gizmos.DrawLine(p0, p1);                                
                // }
                var cellCenter = GridLayout.CellToWorld(cell) + GridLayout.cellSize / 2f;
                Gizmos.DrawCube(cellCenter, (_gridLayout.cellSize * 3f) / 4f);
            }
        }
    }
}
