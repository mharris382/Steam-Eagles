using System;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace Buildables
{
    public abstract class BuildableMachineBase : MonoBehaviour
    {
        private GridLayout _gridLayout;

        [ValidateInput(nameof(ValidateBuildingTarget))] [PropertyOrder(-9)]
        public GameObject buildingTarget;

        [Required] [SerializeField] private Transform partsParent;
        [SerializeField] private FX fx;

        [Serializable]
        public class FX
        {
            public GameFX buildFX;
            public GameFX destroyFX;
        }

        public bool snapsToGround = true;
        private bool _isFlipped;

        public string machineAddress;

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

                return (Vector2Int)GridLayout.WorldToCell(transform.position);
            }
        }


        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                transform.localScale = new Vector3(_isFlipped ? -1 : 1, 1, 1);
                //partsParent.localScale = new Vector3(_flipped ? -1 : 1, 1, 1);
                //partsParent.localPosition = new Vector3(_flipped ? -MachineGridSize.x : 0, MachineGridSize.y/2f, 0);
            }
        }

        public GridLayout GridLayout
        {
            get
            {
                if (_gridLayout != null) return _gridLayout;
                if (!HasResources())
                {
                    Debug.LogError("No grid found on building target or building not set", this);
                    return null;
                }

                _gridLayout = FindGridOnTarget();
                return _gridLayout;
            }
        }

        public bool HasResources()
        {
            if (_gridLayout != null)
                return true;

            if (buildingTarget == null)
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
                    Debug.LogError($"No grid or tilemap found on building target {buildingTarget.name}",
                        buildingTarget);
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


        public void CopySizeOntoPreviewSprite(SpriteRenderer previewSpriteRenderer)
        {
            previewSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            var size = MachineGridSize;
            previewSpriteRenderer.size = new Vector2(size.x, size.y);
            previewSpriteRenderer.transform.localScale = new Vector3(IsFlipped ? -1 : 1, 1, 1);

            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                previewSpriteRenderer.sprite = sr.sprite;
            }
        }

        public bool IsPlacementValid(Building building, Vector3Int cell)
        {
            //if flipped, offset position by size x
            if (this.IsFlipped)
            {
                cell -= new Vector3Int(MachineGridSize.x, 0, 0);
            }

            var size = this.MachineGridSize;
            
            //check that all cells are empty
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var cellPos = cell + new Vector3Int(x, y, 0);
                    var valid = IsCellValid(building, cellPos);
                    //Debug.DrawLine(sPos, building.Map.CellToWorld(cellPos, BuildingLayers.SOLID), valid ? Color.blue : Color.yellow, .5f);
                    if (!valid)
                    {
                        return false;
                    }
                }
            }

            if (snapsToGround)
            {
                if (!IsPlacementOnGround(building, cell, size)) return false;
            }

            return true;
        }

        protected virtual bool IsCellValid(Building building, Vector3Int cellPos)
        {
            var tile = building.Map.GetTile(cellPos, BuildingLayers.SOLID);
            if (tile == null) building.Map.GetTile(cellPos, BuildingLayers.FOUNDATION);
            var room = building.Map.GetRoom(cellPos, BuildingLayers.SOLID);
            if (room == null || room.buildLevel != BuildLevel.FULL)
                return false;
            if (tile != null)
                return false;
            return true;
        }

        /// <summary>
        /// checks that all cells beneath the given cell are solid or foundation
        /// </summary>
        /// <param name="building">machines must be placed on building</param>
        /// <param name="cell">cell that corresponds to the bottom left corner of the machine</param>
        /// <param name="machineSize">size of machine in cell space (assumes cell space is BuildingLayers.SOLID <see cref="BuildingLayers"/></param>
        /// <returns></returns>
        private static bool IsPlacementOnGround(Building building, Vector3Int cell, Vector2Int machineSize)
        {
            var floorCheckStart = cell + Vector3Int.down;
            var floorCheckEnd = floorCheckStart + new Vector3Int(machineSize.x, 0, 0);
            var wpS = building.Map.CellToWorld(floorCheckStart, BuildingLayers.SOLID);
            for (int x = floorCheckStart.x; x < floorCheckEnd.x; x++)
            {
                var check = new Vector3Int(x, floorCheckStart.y, 0);
                var wp = building.Map.CellToWorld(check, BuildingLayers.SOLID);
                var tile = building.Map.GetTile(check, BuildingLayers.SOLID);
                if (tile == null) tile = building.Map.GetTile(check, BuildingLayers.FOUNDATION);
                if (tile == null)
                {
                    return false;
                }
            }

            return true;
        }

        public BuildableMachineBase Build(Vector3Int cell, Building building)
        {
            //if flipped, offset position by size x
            // if (this.Flipped)
            // {
            //     cell -= new Vector3Int(MachineGridSize.x, 0, 0);
            // }
            var pos = building.Map.CellToLocal(cell, BuildingLayers.SOLID);
            var instance = Instantiate(this, pos, Quaternion.identity, building.transform);
            instance.transform.localScale = new Vector3(this.IsFlipped ? -1 : 1, 1, 1);
            instance.buildingTarget = building.gameObject;
            
            var parts = instance.GetComponentsInChildren<BuildableMachinePart>();
            
            foreach (var part in parts) 
                part.OnBuild(building);
            
            if(fx.buildFX !=null)
                fx.buildFX.SpawnEffectFrom(instance.transform);
            return instance;
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
                    Debug.LogWarning("No Building found in scene", this);
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
                    new Vector3(worldPosition.x + gridCellSize.x, worldPosition.y, 0),
                    new Vector3(worldPosition.x + gridCellSize.x, worldPosition.y + gridCellSize.y, 0),
                    new Vector3(worldPosition.x, worldPosition.y + gridCellSize.y, 0),
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