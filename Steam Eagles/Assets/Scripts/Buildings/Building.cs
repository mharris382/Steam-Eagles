using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using Buildings.Rooms;
using Buildings.Tiles;
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

        [OnValueChanged(nameof(UpdateName))]
        public string buildingName;
        
        public Transform tilemapParent;
        
        public int orderInLayer;
        
        public Rect sizeWorldSpace;

        #endregion

        #region [Private Fields]

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
        
        internal Subject<BuildingTilemapChangedInfo> tilemapChangedSubject = new Subject<BuildingTilemapChangedInfo>();

        #endregion
        
        #region [Properties]

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
        public bool IsFullyLoaded
        {
            get;
            set;
        }
        public BuildingMap Map => _buildingMap ??= new BuildingMap(this, GetComponentsInChildren<Room>());
        public BuildingTiles Tiles => _buildingTiles ??= new BuildingTiles(this);
        public string ID => string.IsNullOrEmpty(buildingName) ? name : buildingName;
        public StructureState State => _structureState ? _structureState : _structureState = GetComponent<StructureState>();
        public Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();
        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        public IObservable<BuildingTilemapChangedInfo> TilemapChanged => tilemapChangedSubject;

        public FoundationTilemap FoundationTilemap => _foundationTilemap ? _foundationTilemap : _foundationTilemap = GetComponentInChildren<FoundationTilemap>();
        public SolidTilemap SolidTilemap => _solidTilemap ? _solidTilemap : _solidTilemap = GetComponentInChildren<SolidTilemap>();
        public PipeTilemap PipeTilemap => _pipeTilemap ? _pipeTilemap : _pipeTilemap = GetComponentInChildren<PipeTilemap>();
        public CoverTilemap CoverTilemap => _coverTilemap ? _coverTilemap : _coverTilemap = GetComponentInChildren<CoverTilemap>();
        public WallTilemap WallTilemap => (_wallTilemap)  ? _wallTilemap : _wallTilemap = GetComponentInChildren<WallTilemap>();

        public PlatformTilemap PlatformTilemap => (_platformTilemap) ? _platformTilemap : _platformTilemap = GetComponentInChildren<PlatformTilemap>();
        
        public WireTilemap WireTilemap => (_wireTilemap) ? _wireTilemap : _wireTilemap = GetComponentInChildren<WireTilemap>();
        
        public LadderTilemap LadderTilemap => (_ladderTilemap) ? _ladderTilemap : _ladderTilemap = GetComponentInChildren<LadderTilemap>();
        public DecorTilemap DecorTilemap => (_decorTilemap) ? _decorTilemap : _decorTilemap = GetComponentInChildren<DecorTilemap>();

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

        #endregion
        
        #region [MonoBehaviour Events]

        private void Awake()
        {
            _buildingTiles ??= new BuildingTiles(this);
            _buildingMap ??= new BuildingMap(this, GetComponentsInChildren<Room>());
            _rb = GetComponent<Rigidbody2D>();
            _structureState = GetComponent<StructureState>();
            _box = GetComponent<BoxCollider2D>();
            
            _wallTilemap = GetComponent<WallTilemap>();
            _foundationTilemap = GetComponent<FoundationTilemap>();
            _solidTilemap = GetComponent<SolidTilemap>();
            _pipeTilemap = GetComponent<PipeTilemap>();
            _coverTilemap = GetComponent<CoverTilemap>();
            
            SetupPhysics();
            
            void SetupPhysics()
            {
                gameObject.layer = LayerMask.NameToLayer("Triggers");
                _box.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            PersistenceManager.Instance.GameSaved += SaveBuilding;
            PersistenceManager.Instance.GameLoaded += LoadBuilding;
        }

        private void OnDisable()
        {
            PersistenceManager.Instance.GameSaved -= SaveBuilding;
            PersistenceManager.Instance.GameLoaded -= LoadBuilding;
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

        public IEnumerable<BuildingTilemap> GetAllBuildingLayers() => GetComponentsInChildren<BuildingTilemap>();

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
            this.sizeWorldSpace.DrawGizmos();
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
        
        
        Rooms.Rooms _rooms;
        public Rooms.Rooms Rooms => _rooms ? _rooms : _rooms = GetComponentInChildren<Rooms.Rooms>();
    }

    public class BuildingTiles
    {
        private readonly Building _building;
        
        public bool isReady { get; private set; }
        
        private Dictionary<BuildingLayers, List<TileBase>> _loadedTiles = new Dictionary<BuildingLayers, List<TileBase>>();


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
                await UniTask.WhenAll(solidTileLoadOp.ToUniTask(), wireTileLoadOp.ToUniTask(), wallTileLoadOp.ToUniTask(), damagedWallTileLoadOp.ToUniTask(), pipeTileLoadOp.ToUniTask());
                _loadedTiles[BuildingLayers.SOLID].Add(solidTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WIRES].Add(wireTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WALL].Add(wallTileLoadOp.Result);
                _loadedTiles[BuildingLayers.WALL].Add(damagedWallTileLoadOp.Result);
                _loadedTiles[BuildingLayers.PIPE].Add(pipeTileLoadOp.Result);
                isReady = true;
            }));
        }
    }



    public struct BuildingCell
    {
        public Vector3Int cell;
        public BuildingLayers layers;
        
        public BuildingCell(Vector3Int cell, BuildingLayers layers)
        {
            this.cell = cell;
            this.layers = layers;
        }
    }
}
