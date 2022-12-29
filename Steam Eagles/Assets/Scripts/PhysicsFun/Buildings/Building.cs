using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using CoreLib;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using PhysicsFun;
using UnityEditor;
using World;
#endif
namespace Buildings
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BuildingState))]
    public  class Building : MonoBehaviour
    {
        public string buildingName;
        public int orderInLayer;

        
        private Grid _grid;

        public Rect sizeWorldSpace;
        
        public Grid grid => _grid ? _grid : _grid = GetComponent<Grid>();

        private Rigidbody2D _rb;
        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
        private WallTilemap _wallTilemap;
        private FoundationTilemap _foundationTilemap;
        private SolidTilemap _solidTilemap;
        private PipeTilemap _pipeTilemap;
        private CoverTilemap _coverTilemap;
        
        private WallTilemap[] _wallTilemaps;
        private FoundationTilemap[] _foundationTilemaps;
        private SolidTilemap[] _solidTilemaps;
        private PipeTilemap[] _pipeTilemaps;
        private CoverTilemap[] _coverTilemaps;
        
        public FoundationTilemap[] FoundationTilemaps => (_wallTilemaps==null ||_wallTilemaps.Length < 1)  ? _foundationTilemaps : _foundationTilemaps = GetComponentsInChildren<FoundationTilemap>();
        public SolidTilemap[] SolidTilemaps => (_wallTilemaps==null ||_wallTilemaps.Length < 1)  ? _solidTilemaps : _solidTilemaps = GetComponentsInChildren<SolidTilemap>();
        public PipeTilemap[] PipeTilemaps => (_wallTilemaps==null ||_wallTilemaps.Length < 1)  ? _pipeTilemaps : _pipeTilemaps = GetComponentsInChildren<PipeTilemap>();
        public CoverTilemap[] CoverTilemaps => (_wallTilemaps==null ||_wallTilemaps.Length < 1)  ? _coverTilemaps : _coverTilemaps = GetComponentsInChildren<CoverTilemap>();
        public WallTilemap[] WallTilemaps => (_wallTilemaps==null ||_wallTilemaps.Length < 1)  ? _wallTilemaps : _wallTilemaps = GetComponentsInChildren<WallTilemap>();
        
        public FoundationTilemap FoundationTilemap => _foundationTilemap ? _foundationTilemap : _foundationTilemap = GetComponentInChildren<FoundationTilemap>();
        public SolidTilemap SolidTilemap => _solidTilemap ? _solidTilemap : _solidTilemap = GetComponentInChildren<SolidTilemap>();
        public PipeTilemap PipeTilemap => _pipeTilemap ? _pipeTilemap : _pipeTilemap = GetComponentInChildren<PipeTilemap>();
        public CoverTilemap CoverTilemap => _coverTilemap ? _coverTilemap : _coverTilemap = GetComponentInChildren<CoverTilemap>();
        public WallTilemap WallTilemap => (_wallTilemap)  ? _wallTilemap : _wallTilemap = GetComponentInChildren<WallTilemap>();

        public bool HasResources
        {
            get
            {
                return WallTilemap != null
                       && FoundationTilemap != null
                       && SolidTilemap != null
                       && PipeTilemap != null
                       && CoverTilemap != null;
            }
        }

        private void Awake()
        {
            UpdateRigidbody(Rb);
        }

        public void Update()
        {
            if(Application.isPlaying) return;
            UpdateTilemaps();
        }


        public void UpdateTilemaps()
        {
            foreach (var buildingLayer in GetAllBuildingLayers())
            {
                buildingLayer.UpdateTilemap(this);
            }
        }

        IEnumerable<BuildingTilemap> GetAllBuildingLayers()
        {
            return GetComponentsInChildren<BuildingTilemap>();
            List<BuildingTilemap> lob = new List<BuildingTilemap>();
            
            lob.AddRange(WallTilemaps);
            lob.AddRange(PipeTilemaps);
            lob.AddRange(SolidTilemaps);
            lob.AddRange(FoundationTilemaps);
            lob.AddRange(CoverTilemaps);
            return lob;
        }

        public IEnumerator<BuildingTilemap> GetMapSortingOrder_BackToFront()
        {
            yield return WallTilemap;
            yield return PipeTilemap;
            yield return SolidTilemap;
            yield return FoundationTilemap;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            this.sizeWorldSpace.DrawGizmos();
        }

        public void CalculateSize()
        {
            var bounds = new Bounds();
            foreach (var buildingLayer in GetAllBuildingLayers())
            {
                var layerBounds = buildingLayer.GetWorldBounds();
                bounds.Encapsulate(layerBounds.max);
                bounds.Encapsulate(layerBounds.min);
            }
        
            this.sizeWorldSpace = bounds.ToRect();
        }

        public virtual void UpdateRigidbody(Rigidbody2D rb)
        {
            rb.bodyType = RigidbodyType2D.Static;
            
        }
    }


#if UNITY_EDITOR
    
    [CustomEditor(typeof(Building), true)]
    public class BuildingEditor : Editor
    {
        public static BuildingTilemap CreateBuildingTilemapType(Building building, BuildingLayers type)
        {
            
            var go = new GameObject($"{building}_{type.ToString().ToLower()}", typeof(Tilemap), typeof(TilemapRenderer));

            TilemapCollider2D AddCollider( bool asPlatform =false, bool asTrigger =false)
            {
                var collider = go.AddComponent<TilemapCollider2D>();
                collider.isTrigger = asTrigger;
                if (asPlatform)
                {
                    collider.usedByEffector = asPlatform;
                    go.AddComponent<PlatformEffector2D>();
                }
                return collider;
            }
            switch (type)
            {
                case BuildingLayers.SOLID:
                    AddCollider();
                    go.layer = LayerMask.NameToLayer("Solids");
                    go.tag = "Solid Tilemap";
                    return go.AddComponent<SolidTilemap>();
                    break;
                
                case BuildingLayers.WALL:
                    go.layer = LayerMask.NameToLayer("Air");
                    go.tag = "Wall";
                    return go.AddComponent<WallTilemap>();
                    break;
                
                case BuildingLayers.FOUNDATION:
                    go.layer = LayerMask.NameToLayer("Ground");
                    go.tag = "Solid Tilemap";
                    AddCollider();
                    return go.AddComponent<FoundationTilemap>();

                case BuildingLayers.PLATFORM:
                    AddCollider(asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Platforms");
                    return go.AddComponent<PlatformTilemap>();
                
                case BuildingLayers.PIPE:
                    AddCollider(asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Pipes");
                    go.tag = "Pipe Tilemap";
                    return go.AddComponent<PipeTilemap>();
                
                case BuildingLayers.COVER:
                    AddCollider(asTrigger: true);
                    go.layer = LayerMask.NameToLayer("Triggers");
                    go.AddComponent<BuildingFaderTrigger>();
                    return go.AddComponent<CoverTilemap>();

                case BuildingLayers.DECOR:
                    throw new NotImplementedException();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

      
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var building = target as Building;
            void AddToBuilding(BuildingTilemap buildingTilemap)
            {
                buildingTilemap.transform.parent = building.transform;
                
            }
            if (!building.HasResources)
            {
                EditorGUILayout.HelpBox($"Building {building.name} is missing resources", MessageType.Warning);
                if (GUILayout.Button("Create Tilemaps"))
                {
                    if (building.SolidTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, BuildingLayers.SOLID));
                    if (building.WallTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, BuildingLayers.WALL));
                    if (building.FoundationTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, BuildingLayers.FOUNDATION));
                    if (building.PipeTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, BuildingLayers.PIPE));
                    if (building.CoverTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, BuildingLayers.COVER));
                }
            }
            else
            {
                if (GUILayout.Button("Update Size"))
                {
                    building.CalculateSize();
                }
            }
            
            building.UpdateTilemaps();
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
#endif
}