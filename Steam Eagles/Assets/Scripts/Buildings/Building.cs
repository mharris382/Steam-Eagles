﻿using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using Buildings.Rooms;
using Buildings.Tiles;
using Buildings.Tiles.Skin;
using CoreLib;
using CoreLib.SaveLoad;
using Cysharp.Threading.Tasks;
using PhysicsFun.Buildings;
using SaveLoad;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using World;
using Zenject;

namespace Buildings
{




    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    [RequireComponent(typeof(StructureState), typeof(BuildingPlayerTracker))]
    public class Building : MonoBehaviour, IStructure, IBuildingTilemaps
    {
        #region [Inspector Fields]

        [OnValueChanged(nameof(UpdateName))] public string buildingName;

        public Transform tilemapParent;

        public int orderInLayer;

        [Button]
        void Helper()
        {
            Debug.Log(typeof(Building).AssemblyQualifiedName);
            var assemblyQualifiedName = typeof(Building).AssemblyQualifiedName;
            if (assemblyQualifiedName != null)
            {
                var t = Type.GetType(assemblyQualifiedName);
                Debug.Assert(t != null);
            }
        }
        
        [SerializeField] Rect _sizeWorldSpace;

        
        public bool useBoxColliderSize = true;
        
        #endregion

        #region [Private Fields]

        private TileSkin _skin;
        private StructureState _structureState;
        private Grid _grid;
        private Rigidbody2D _rb;
        private BoxCollider2D _box;

        private WallTilemap _wallTilemap;
        private FoundationTilemap _foundationTilemap;
        private SolidTilemap _solidTilemap;
        private PipeTilemap _pipeTilemap;
        private CoverTilemap _coverTilemap;
        private PlatformTilemap _platformTilemap;
        private WireTilemap _wireTilemap;
        private LadderTilemap _ladderTilemap;
        private DecorTilemap _decorTilemap;

        private BuildingMap _buildingMap;
        private BuildingTiles _buildingTiles;
        Rooms.Rooms _rooms;
        
        [SerializeField]
        RectInt _sizeGridSpace;

        internal Subject<BuildingTilemapChangedInfo> tilemapChangedSubject = new Subject<BuildingTilemapChangedInfo>();

        #endregion

        #region [Properties]

        public Transform TilemapParent
        {
            get
            {
                if (tilemapParent == null)
                {
                    tilemapParent = new GameObject("[TILEMAPS]").transform;
                    tilemapParent.SetParent(transform);
                    tilemapParent.localPosition = Vector3.zero;
                }
                return tilemapParent;
            }
        }


        public RectInt SizeGridSpace
        {
            get => _sizeGridSpace;
            set
            {
                _sizeGridSpace = value;
                var minGs = (Vector3Int)_sizeGridSpace.min;
                var maxGs = (Vector3Int)_sizeGridSpace.max;
                var minWs = Grid.CellToWorld(minGs);
                var maxWs = Grid.CellToWorld(maxGs);
                _sizeWorldSpace = new Rect(minWs, maxWs - minWs);
            }
        }

        public Rect sizeWorldSpace
        {
            get => _sizeWorldSpace;
        }
        
        public Bounds WorldSpaceBounds
        {
            get
            {
                var bounds = new Bounds(transform.position + (Vector3)sizeWorldSpace.center, sizeWorldSpace.size);
                var s = bounds.size;
                s.z = 10;
                bounds.size = s;
                return bounds;
            }
        }

        public bool IsFullyLoaded { get; set; }

        public BuildingMap Map => _buildingMap ??= new BuildingMap(this);
        public BuildingTiles Tiles => _buildingTiles ??= new BuildingTiles(this);
        public string ID => string.IsNullOrEmpty(buildingName) ? name : buildingName;

        public StructureState State =>
            _structureState ? _structureState : _structureState = GetComponent<StructureState>();

        public Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();
        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
        
        private BoxCollider2D Box => _box ? _box : _box = GetComponent<BoxCollider2D>();

        public IObservable<BuildingTilemapChangedInfo> TilemapChanged => tilemapChangedSubject;

        public FoundationTilemap FoundationTilemap => _foundationTilemap
            ? _foundationTilemap
            : _foundationTilemap = GetComponentInChildren<FoundationTilemap>();

        public SolidTilemap SolidTilemap =>
            _solidTilemap ? _solidTilemap : _solidTilemap = GetComponentInChildren<SolidTilemap>();

        public PipeTilemap PipeTilemap =>
            _pipeTilemap ? _pipeTilemap : _pipeTilemap = GetComponentInChildren<PipeTilemap>();

        public CoverTilemap CoverTilemap =>
            _coverTilemap ? _coverTilemap : _coverTilemap = GetComponentInChildren<CoverTilemap>();

        public WallTilemap WallTilemap =>
            (_wallTilemap) ? _wallTilemap : _wallTilemap = GetComponentInChildren<WallTilemap>();

        public PlatformTilemap PlatformTilemap => (_platformTilemap)
            ? _platformTilemap
            : _platformTilemap = GetComponentInChildren<PlatformTilemap>();

        public WireTilemap WireTilemap =>
            (_wireTilemap) ? _wireTilemap : _wireTilemap = GetComponentInChildren<WireTilemap>();

        public LadderTilemap LadderTilemap =>
            (_ladderTilemap) ? _ladderTilemap : _ladderTilemap = GetComponentInChildren<LadderTilemap>();

        public DecorTilemap DecorTilemap =>
            (_decorTilemap) ? _decorTilemap : _decorTilemap = GetComponentInChildren<DecorTilemap>();


        public BuildingTilemap GetTilemap(BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.WALL:
                    return WallTilemap;
                    break;
                case BuildingLayers.FOUNDATION:
                    return FoundationTilemap;
                    break;
                case BuildingLayers.SOLID:
                    return SolidTilemap;
                    break;
                case BuildingLayers.PIPE:
                    return PipeTilemap;
                    break;
                case BuildingLayers.COVER:
                    return CoverTilemap;
                    break;
                case BuildingLayers.PLATFORM:
                    return PlatformTilemap;
                    break;
                case BuildingLayers.DECOR:
                    return DecorTilemap;
                    break;
                case BuildingLayers.WIRES:
                    return WireTilemap;
                    break;
                case BuildingLayers.LADDERS:
                    return LadderTilemap;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }

        public bool HasResources =>
            WallTilemap != null
            && FoundationTilemap != null
            && SolidTilemap != null
            && PipeTilemap != null
            && CoverTilemap != null;

        #endregion

        #region [Injection Methods]

        [Inject]
        public void RegistryBuilding(BuildingRegistry buildingRegistry)
        {
            buildingRegistry.AddBuilding(this);
            this.OnDestroyAsObservable().Subscribe(_ => buildingRegistry.RemoveBuilding(this));
        }

    





        [Inject]
        public void AssignSkin(TileSkin tileSkin)
        {
            this._skin = tileSkin;
            UpdateSkin();
        }

    
        
        #endregion

        #region [MonoBehaviour Events]

        private void Awake()
        {
            _buildingTiles ??= new BuildingTiles(this);
            _rb = GetComponent<Rigidbody2D>();
            _structureState = GetComponent<StructureState>();
            _box = GetComponent<BoxCollider2D>();

            _wallTilemap = GetComponent<WallTilemap>();
            _foundationTilemap = GetComponent<FoundationTilemap>();
            _solidTilemap = GetComponent<SolidTilemap>();
            _pipeTilemap = GetComponent<PipeTilemap>();
            _coverTilemap = GetComponent<CoverTilemap>();

            SetupPhysics();
            CalculateSizeWs();

            void SetupPhysics()
            {
                gameObject.layer = LayerMask.NameToLayer("Triggers");
                _box.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            // PersistenceManager.Instance.GameSaved += SaveBuilding;
            // PersistenceManager.Instance.GameLoaded += LoadBuilding;
        }

        private void OnDisable()
        {
           // PersistenceManager.Instance.GameSaved -= SaveBuilding;
           // PersistenceManager.Instance.GameLoaded -= LoadBuilding;
        }

        #endregion

        #region [Public Methods]

        public void SaveBuilding(string path)
        {
            //TODO: trigger save
            Debug.Log("TODO: Saving Building".Bolded());
        }


        public void LoadBuilding(string path)
        {
            //TODO: trigger load
            Debug.Log("TODO: Load Building".Bolded());
        }

        #endregion

        #region [Helper Methods]


        private void CalculateSizeWs()
        {
            var rooms = GetComponent<Rooms.Rooms>();
            var max = Vector2Int.zero;
            var min = Vector2Int.zero;
            if (rooms == null) return;
            foreach (var room in rooms.AllRooms)
            {
                var bounds = room.RoomRect;
                if (bounds.xMin < min.x) min.x = bounds.xMin;
                if (bounds.yMin < min.y) min.y = bounds.yMin;
                if (bounds.xMax > max.x) max.x = bounds.xMax;
                if (bounds.yMax > max.y) max.y = bounds.yMax;
            }

            var finalRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            SizeGridSpace = finalRect.CastTo();
        }
        public IEnumerable<BuildingTilemap> GetAllBuildingLayers() => GetComponentsInChildren<BuildingTilemap>();

        [Button, HideInEditorMode, ShowIf("@_skin != null")]
        private void UpdateSkin()
        {
            _skin.ApplySkin(this);
        }

        #endregion

        #region [Editor Stuff]

        private void UpdateName(string n)
        {
            var st = string.IsNullOrEmpty(n) ? name : n;
            Dictionary<Type, List<BuildingTilemap>> buildingTilemaps = new Dictionary<Type, List<BuildingTilemap>>();
            foreach (var buildingTilemap in GetAllBuildingLayers())
            {
                var type = buildingTilemap.GetType();
                if (!buildingTilemaps.ContainsKey(type))
                    buildingTilemaps.Add(type, new List<BuildingTilemap>());
                buildingTilemaps[type].Add(buildingTilemap);
            }

            foreach (var kvp in buildingTilemaps)
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var buildingTilemap = kvp.Value[i];
                    if (kvp.Value.Count > 1)
                    {
                        buildingTilemap.name = $"{st} {kvp.Key.Name} {i}";
                    }
                    else
                    {
                        buildingTilemap.name = $"{st} {kvp.Key.Name}";
                    }
                }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            this.WorldSpaceBounds.ToRect().DrawGizmos();
        }
#endif

        #endregion

        public bool HasTile(Vector2Int cellPosition, BuildingLayers layers)
        {
            int layer = (int)layers;

            foreach (var buildingTilemap in GetAllBuildingLayers())
            {
                if (HasLayer(layer, buildingTilemap.Layer))
                {
                    if (buildingTilemap.Tilemap.HasTile(new Vector3Int(cellPosition.x, cellPosition.y, 0)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryGetTile(Vector2Int cellPosition, BuildingLayers layers, out TileBase tile)
        {
            int layer = (int)layers;
            tile = null;
            foreach (var buildingTilemap in GetAllBuildingLayers())
            {
                if (HasLayer(layer, buildingTilemap.Layer))
                {
                    var cell = new Vector3Int(cellPosition.x, cellPosition.y, 0);
                    if (buildingTilemap.Tilemap.HasTile(cell))
                    {
                        tile = buildingTilemap.Tilemap.GetTile(cell);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasLayer(int layer, BuildingLayers layers) => ((layer & (int)layers) != 0);


        public Rooms.Rooms Rooms => _rooms ? _rooms : _rooms = GetComponentInChildren<Rooms.Rooms>();
    }

    public class BuildingTiles
    {
        private readonly Building _building;

        public bool isReady { get; private set; }

        private Dictionary<BuildingLayers, List<TileBase>> _loadedTiles =
            new Dictionary<BuildingLayers, List<TileBase>>();


        public TileBase GetDefaultTile(BuildingLayers layers) => _loadedTiles[layers][0];

        public TileBase DamagedWallTile => _loadedTiles[BuildingLayers.WALL][1];
        public TileBase WallTile => _loadedTiles[BuildingLayers.WALL][0];
        public TileBase SolidTile => _loadedTiles[BuildingLayers.SOLID][0];
        public TileBase PipeTile => _loadedTiles[BuildingLayers.PIPE][0];
        public TileBase WireTile => _loadedTiles[BuildingLayers.WIRES][0];

        public BuildingTiles(Building building)
        {
            this._building = building;
            _loadedTiles.Add(BuildingLayers.WALL, new List<TileBase>());
            _loadedTiles.Add(BuildingLayers.SOLID, new List<TileBase>());
            _loadedTiles.Add(BuildingLayers.PIPE, new List<TileBase>());
            _loadedTiles.Add(BuildingLayers.WIRES, new List<TileBase>());
            isReady = false;
            building.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var solidTileLoadOp = Addressables.LoadAssetAsync<SolidTile>("SolidTile");
                var wireTileLoadOp = Addressables.LoadAssetAsync<WireTile>("WireTile");
                var wallTileLoadOp = Addressables.LoadAssetAsync<WallTile>("WallTile");
                var damagedWallTileLoadOp = Addressables.LoadAssetAsync<DamagedWallTile>("DamagedWallTile");
                var pipeTileLoadOp = Addressables.LoadAssetAsync<PipeTile>("PipeTile");
                await UniTask.WhenAll(solidTileLoadOp.ToUniTask(), wireTileLoadOp.ToUniTask(),
                    wallTileLoadOp.ToUniTask(), damagedWallTileLoadOp.ToUniTask(), pipeTileLoadOp.ToUniTask());
                _loadedTiles[BuildingLayers.SOLID].Add(solidTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WIRES].Add(wireTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WALL].Add(wallTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WALL].Add(damagedWallTileLoadOp.Result);
                _loadedTiles[BuildingLayers.PIPE].Add(pipeTileLoadOp.Result);
                isReady = true;
            }));
        }
    }


    public class BuildingTileLookup
    {
        class LayerTileLookup
        {
            private readonly BuildingLayers _layers;

            public LayerTileLookup(Building building, BuildingLayers layers)
            {
                _layers = layers;
            }
        }
    }


    public struct BuildingCell : IEquatable<BuildingCell>, IComparable<BuildingCell>
    {
        public Vector3Int cell;
        public BuildingLayers layers;
        public Vector2Int cell2D => (Vector2Int)cell;

        public BuildingCell(Vector3Int cell, BuildingLayers layers)
        {
            this.cell = cell;
            this.layers = layers;
        }

        public BuildingCell(Vector2Int cell, BuildingLayers layers)
        {
            this.cell = (Vector3Int)cell;
            this.layers = layers;
        }

        public IEnumerable<BuildingCell> GetNeighbors()
        {
            yield return this + Vector2Int.up;
            yield return this + Vector2Int.right;
            yield return this + Vector2Int.left;
            yield return this + Vector2Int.down;
        }

        public void ClearZ()
        {
            this.cell.z = 0;
        }

    public bool Equals(BuildingCell other) => layers == other.layers && cell.Equals(other.cell);
        public override bool Equals(object obj) => obj is BuildingCell other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(cell2D, (int)layers);
        
        public static bool operator ==(BuildingCell left, BuildingCell right) => left.Equals(right);
        public static bool operator !=(BuildingCell left, BuildingCell right) => !(left == right);
        
        public static implicit operator Vector2Int(BuildingCell cell) => cell.cell2D;
        public static explicit operator Vector3Int(BuildingCell cell) => cell.cell;

        public override string ToString()
        {
            return $"({cell.x}, {cell.y}, {layers})";
        }


        public static BuildingCell operator +(BuildingCell buildingCell, Vector2Int vector2Int) => new BuildingCell(buildingCell.cell + (Vector3Int)vector2Int, buildingCell.layers);
        public static BuildingCell operator +(BuildingCell buildingCell, Vector3Int vector2Int) => new BuildingCell(buildingCell.cell + (Vector3Int)vector2Int, buildingCell.layers);

        public int CompareTo(BuildingCell other)
        {
            if(other.layers != this.layers) return (int)other.layers - (int)layers > 0 ? 1 : -1;
            if (other.cell2D == cell2D) return 0;
            return cell.x > other.cell.x ? 1 : cell.x < other.cell.x ? -1 : cell.y > other.cell.y ? 1 : cell.y < other.cell.y ? -1 : 0;
        }
    }

    public struct BuildingTile : IEquatable<BuildingTile>
    {
        public BuildingCell cell;
        public TileBase tile;
        
        public bool IsEmpty => tile == null;

        public BuildingLayers Layer
        {
            get => cell.layers;
            set => cell.layers = value;
        }

        public BuildingTile(Vector3Int cell, BuildingLayers layer, TileBase tile)
        {
            this.cell = new BuildingCell(cell, layer);
            this.tile = tile;
        }
        public BuildingTile(BuildingCell cell, TileBase tile)
        {
            this.cell = cell;
            this.tile = tile;
        }

        public bool Equals(BuildingTile other) => cell.Equals(other.cell) && Equals(tile, other.tile);
        public override bool Equals(object obj) => obj is BuildingTile other && Equals(other);
        public override int GetHashCode() => cell.GetHashCode();
        
        public static bool operator ==(BuildingTile left, BuildingTile right) => left.Equals(right);
        public static bool operator !=(BuildingTile left, BuildingTile right) => !(left == right);

        public static implicit operator BuildingCell(BuildingTile buildingTile) => buildingTile.cell;
        public static explicit operator BuildingTile(BuildingCell buildingCell) => new(buildingCell, null);

        public static implicit operator Vector2Int(BuildingTile cell) => cell.cell.cell2D;
        public override string ToString() => $"{cell.ToString()} {(tile == null ? "EMPTY" : tile.name)}";
    }

    public struct BuildingRect : IEquatable<BuildingRect>
    {
        public readonly BuildingLayers layers;
        public readonly BoundsInt bounds;

        public int Area2D => bounds.size.x * bounds.size.y;
        public int Area => bounds.size.x * bounds.size.y * bounds.size.z;
        
        public BuildingRect(BoundsInt bounds, BuildingLayers layers)
        {
            this.bounds = bounds;
            this.layers = layers;
        }
        public BuildingRect(Vector3Int position, Vector3Int size, BuildingLayers layers)
        {
            this.bounds = new BoundsInt(position, size);
            this.layers = layers;
        }
        public BuildingRect(BuildingCell cell, Vector3Int size) : this(cell.cell, size, cell.layers) { }
        public bool Equals(BuildingRect other) => layers == other.layers && bounds.Equals(other.bounds);
        public override bool Equals(object obj) => obj is BuildingRect other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)layers, bounds);
        public bool Contains(BuildingCell cell) => cell.layers == this.layers && bounds.Contains(cell.cell);

        public override string ToString()
        {
            return $"Position:\t {(Vector2Int)bounds.position} {(Vector2Int)bounds.size}\n" +
                   $"Layer:\t {layers}";
        }
    }
}
