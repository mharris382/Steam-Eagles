using System;
using Buildings;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables
{
    public abstract class BuildableMachineBase : MonoBehaviour
    {
        private GridLayout _gridLayout;
        
        [ValidateInput(nameof(ValidateBuildingTarget))]
        [PropertyOrder(-9)]
        public GameObject buildingTarget;

        
        /// <summary> how many cells this machine occupies in the grid </summary>
        public abstract Vector2Int MachineGridSize { get; }

        public Vector2Int CellPosition
        {
            get
            {
                if (GridLayout == null)
                {
                    var position = transform.position;
                    return new Vector2Int(Mathf.RoundToInt(position.x),
                        Mathf.RoundToInt(position.y));
                }
                return (Vector2Int) GridLayout.WorldToCell(transform.position);
            }
        }

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

        public bool HasResources()
        {
            if(_gridLayout != null)
                return true;
            
            if(buildingTarget == null) 
                return false;

            _gridLayout = FindGridOnTarget();
            return _gridLayout != null;
        }

        private GridLayout FindGridOnTarget()
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

        
        
        #region [Editor]

        bool HasBuilding => buildingTarget != null;
        [PropertyOrder(-10)]
        [Button, HideIf(nameof(HasBuilding))]
        private void FindBuildingTarget()
        {
            var b = GetComponentInParent<Building>();
            if (b == null)
            {
                b = FindObjectOfType<Building>();
                if (b == null)
                {
                    Debug.LogWarning("No Building found in scene",this);
                    return;
                }
                buildingTarget = b.gameObject;
                this.transform.parent = buildingTarget.transform;
            }
            buildingTarget = b.gameObject;
        }

        private bool ValidateBuildingTarget(GameObject building, ref string error)
        {
            if (building == null)
            {
                error = "No building target set";
            }
            else if (building.GetComponent<Grid>() == null)
            {
                error = "Building target has no grid";
            }
            var b = GetComponentInParent<Building>();
            if (b == null)
            {
                error = "Not a child of building target";
            }
            else if (b.gameObject != building)
            {
                error = $"Machine is child of wrong the building target: {building.name}";
            }

            return true;
        }


        private void OnDrawGizmosSelected()
        {
            if (!HasResources()) return;
            var cellPos = GridLayout.WorldToCell(transform.position);
            var gridCellSize = GridLayout.cellSize;
            Gizmos.color = Color.red.SetAlpha(0.6f);
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
            for (int x = 0; x < MachineGridSize.x; x++)
            {
                for (int y = 0; y < MachineGridSize.y; y++)
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

            //foreach (var machineCell in machineCells)
            //{
            //    if(machineCell.tile == null)
            //        continue;
            //    var cell = cellPos + new Vector3Int(machineCell.cellPosition.x, machineCell.cellPosition.y, 0);
            //    var worldPos = GridLayout.CellToWorld(cell);
            //    var corners = GetCorners(worldPos);
            //    Gizmos.color = machineCell.gizmoColor;
            //    // for (int i = 1; i < corners.Length; i++)                    
            //    // {                                                           
            //    //     var p0 = corners[i - 1];                                
            //    //     var p1 = corners[i];                                    
            //    //     Gizmos.DrawLine(p0, p1);                                
            //    // }
            //    var cellCenter = GridLayout.CellToWorld(cell) + GridLayout.cellSize / 2f;
            //    Gizmos.DrawCube(cellCenter, (_gridLayout.cellSize * 3f) / 4f);
            //}
        }

        #endregion
    }
}