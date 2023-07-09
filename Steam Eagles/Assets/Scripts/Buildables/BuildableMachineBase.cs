using System;
using System.Collections.Generic;
using Buildables.Interfaces;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;
using Zenject;

namespace Buildables
{
    public interface IMachineCustomSaveData
    {
        void LoadDataFromJson(string json);
        string SaveDataToJson();
    }
    public abstract class BuildableMachineBase : Machine, IIconable, IMachineTileProvider
    {
        
        [Serializable]
        public class FX
        {
            public GameFX buildFX;
            public GameFX destroyFX;
        }


        [Inject]
        public void TestInject(BuildablesRegistry buildablesRegistry)
        {
            Debug.Log($"Successfully Injected {name}!".Bolded().ColoredBlue(),this);
            _buildablesRegistry = buildablesRegistry;
        }

        public void SetIsCopy(bool isCopy)
        {
            _isCopy = isCopy;
        }

        private bool _isCopy;
        public bool IsCopy => _isCopy;
        [ValidateInput(nameof(ValidateBuildingTarget))] [PropertyOrder(-9)] public GameObject buildingTarget;
        [SerializeField] private BuildingLayers targetLayer = BuildingLayers.SOLID;

        [SerializeField] [Required] private Transform partsParent;
        [SerializeField] private Sprite previewIcon;
        [SerializeField] private FX fx;
        public bool snapsToGround = true;
        public bool debug = true;
        private GridLayout _gridLayout;
        private SpriteRenderer _sr;
        private Building _building;

        private bool _isFlipped;
        public string machineAddress;
        private int _machineID;
        private Vector3Int? _spawnedPosition;
        private BuildablesRegistry _buildablesRegistry;

        #region [Properties]

        /// <summary> how many cells this machine occupies in the grid </summary>
        public abstract Vector2Int MachineGridSize { get; }

        Vector3Int IMachineTileProvider.CellPosition => (Vector3Int)this.CellPosition; 
        public Vector2Int CellPosition
        {
            get
            {
                if (_spawnedPosition != null)
                {
                    return (Vector2Int)_spawnedPosition.Value;
                }
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
                _gridLayout = FindGridOnTarget();
                if (!HasResources())
                {
                    if(Application.isPlaying)
                        Debug.LogError("No grid found on building target or building not set", this);
                    return null;
                }

                return _gridLayout;
            }
        }

        public Building Building
        {
            get
            {
                if (_building == null) _building = GetComponentInParent<Building>();
                return _building;
            }
        }


        public SpriteRenderer sr => _sr ? _sr : _sr = GetComponent<SpriteRenderer>();
        bool HasBuilding => buildingTarget != null;

        
        public int MachineID
        {
            get => _machineID;
            set
            {
                _machineID = value;
                Debug.Log($"BMachine: {name} assigned ID {value}",this);
            }
        }

        public Vector2Int MachineSize => this.MachineGridSize;
        public virtual IEnumerable<MachineGridArea> GetMachineGridAreas()
        {
            yield return new MachineGridArea(MachineID, MachineSize, MachineLayers.PIPE);
            yield return new MachineGridArea(MachineID, MachineSize, MachineLayers.SOLID);
        }

        #endregion

        public bool HasResources()
        {
            if (_gridLayout != null)
                return true;

            if (buildingTarget == null)
                return false;

            _gridLayout = FindGridOnTarget();
            return _gridLayout != null;
        }

        public BuildingLayers GetTargetLayer() => targetLayer;

        public Sprite GetIcon() => previewIcon != null ? previewIcon : ( sr != null ? sr.sprite : null );

        public IEnumerable<Vector3Int> GetCells(Vector2Int position)
        {
            return GetCells(position, IsFlipped);
        }

        public IEnumerable<Vector3Int> GetBottomCells(Vector2Int position)
        {
            return GetBottomCells(position, IsFlipped);
        }

        public IEnumerable<Vector3Int> GetBottomCells(Vector2Int position, bool flipped)
        {
            var cell = (Vector3Int) position;
            var size = this.MachineGridSize;
            var offset = IsFlipped ? new Vector3Int(-size.x, 0, 0) : Vector3Int.zero;
            for (int x = 0; x < size.x; x++)
            {
                var y = 0;
                var cellPos = cell + new Vector3Int(x, y, 0);
                yield return cellPos + offset;
            }
        }
        public IEnumerable<Vector3Int> GetCells()
        {
            return GetCells(this.CellPosition);
        }


        public IEnumerable<Vector3Int> GetCells(Vector2Int position, bool flipped)
        {
            var cell = (Vector3Int) position;
            var size = this.MachineGridSize;
            var offset = flipped ? new Vector3Int(-size.x, 0, 0) : Vector3Int.zero;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var cellPos = cell + new Vector3Int(x, y, 0);
                    yield return cellPos + offset;
                }
            }
        }

        public void CopyOntoPreviewSprite(SpriteRenderer previewSpriteRenderer)
        {
            previewSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            var size = MachineGridSize;
            previewSpriteRenderer.size = new Vector2(size.x, size.y);
            previewSpriteRenderer.transform.localScale = new Vector3(IsFlipped ? -1 : 1, 1, 1);

            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                previewSpriteRenderer.sprite = GetIcon();
            }
        }
        [System.Obsolete("use BMachines instead")]
        public bool IsPlacementValid(Building building, ref Vector3Int cell, ref string errorMessage)
        {//if flipped, offset position by size x
            _spawnedPosition = cell;
            if (building.IsCellOverlappingMachine(cell))
            {
                errorMessage= "This space is already occupied by another machine";
                return false;
            }
            //if (IsFlipped) cell -= new Vector3Int(MachineGridSize.x, 0, 0);
            if (!CheckCells(building))
            {
                errorMessage = "Cannot place machine here";
                return false;
            }

            if (snapsToGround)
            {
                if (!IsPlacementOnGround(building, cell, MachineGridSize))
                {
                    errorMessage = "BMachine must be placed on ground";
                    return false;
                }
            }
            return true;
        }

        [System.Obsolete("use IsPlacementValid(Building building, ref Vector3Int cell, ref string errorMessage)")]
        public bool IsPlacementValid(Building building, Vector3Int cell)
        {
            _spawnedPosition = cell;
            //if flipped, offset position by size x
            if (this.IsFlipped)
            {
                cell -= new Vector3Int(MachineGridSize.x, 0, 0);
            }

            var size = this.MachineGridSize;
            
            if (!CheckCells(building)) return false;

            if (snapsToGround)
            {
                if (!IsPlacementOnGround(building, cell, size)) return false;
            }

            return true;
        }

        public BuildableMachineBase Build(Vector3Int cell, Building building)
        {
            var pos = building.Map.CellToWorld(cell, BuildingLayers.SOLID);
            var offset = new Vector3(0.01f, 0.01f, 0);
            pos += offset;
            var instance = Instantiate(this, pos, Quaternion.identity, building.transform);
            instance._spawnedPosition = cell;
            instance._isFlipped = this.IsFlipped;
            instance.transform.localScale = new Vector3(this.IsFlipped ? -1 : 1, 1, 1);
            instance.buildingTarget = building.gameObject;
            
            var parts = instance.GetComponentsInChildren<BuildableMachinePart>();
            
            foreach (var part in parts) 
                part.OnBuild(building);
            
            if(fx.buildFX !=null)
                fx.buildFX.SpawnEffectFrom(instance.transform);
            instance.OnMachineBuilt(cell, building);
            return instance;
        }

        protected virtual void OnMachineBuilt(Vector3Int cell, Building building)
        {
            Debug.Log($"{name} built at {cell} on {building.buildingName}",this);
        }

        protected virtual bool IsCellValid(Building building, Vector3Int cellPos)
        {
            _spawnedPosition = cellPos;
            var tile = building.Map.GetTile(cellPos, BuildingLayers.SOLID);
            if (tile == null) building.Map.GetTile(cellPos, BuildingLayers.FOUNDATION);
            var room = building.Map.GetRoom(cellPos, BuildingLayers.SOLID);
            if (room == null || room.buildLevel != BuildLevel.FULL)
                return false;
            if (tile != null)
                return false;
            return true;
        }


        private bool CheckCells(Building building)
        {
            foreach (var ce in GetCells())
            {
                var valid = IsCellValid(building, ce);
                if (!valid)
                    return false;
            }
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

        private GridLayout FindGridOnTarget()
        {
            if (buildingTarget == null)
            {
                var b = GetComponentInParent<Building>();
                if (b == null)
                    return null;
                buildingTarget = b.gameObject;
            }
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


        public virtual void DestroyMachine()
        {
            var listeners = this.GetComponentsInChildren<IMachineListener>();
            foreach (var machineListener in listeners)
            {
                machineListener.OnMachineRemoved(this);
            }
            
            GameObject.Destroy(gameObject);
        }
        
        
        public virtual Vector2 GetDropperSpawnPosition()
        {
            var pos = buildingTarget.transform.position;
            pos += new Vector3(0, 0.5f, 0);
            
            return pos;
        }

        #region [Editor]

        [PropertyOrder(-10)] [Button, HideIf(nameof(HasBuilding))]
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
                error = $"BMachine is child of wrong the building target: {building.name}";
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
                    new Vector3(worldPosition.x + (!IsFlipped ? gridCellSize.x : -gridCellSize.x), worldPosition.y, 0),
                    new Vector3(worldPosition.x + (!IsFlipped ? gridCellSize.x : -gridCellSize.x), worldPosition.y + gridCellSize.y, 0),
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