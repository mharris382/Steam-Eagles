using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Buildings
{
    
    [RequireComponent(typeof(Grid))]
    public  class Building : MonoBehaviour
    {
        private Grid _grid;

        public Rect sizeWorldSpace;

        public Grid grid => _grid ? _grid : _grid = GetComponent<Grid>();
        
        public WallTilemap WallTilemap => _wallTilemap ? _wallTilemap : _wallTilemap = GetComponentInChildren<WallTilemap>();
        public FoundationTilemap FoundationTilemap => _oundationTilemap ? _oundationTilemap : _oundationTilemap = GetComponentInChildren<FoundationTilemap>();
        public SolidTilemap SolidTilemap => _olidTilemap ? _olidTilemap : _olidTilemap = GetComponentInChildren<SolidTilemap>();
        public PipeTilemap PipeTilemap => _ipeTilemap ? _ipeTilemap : _ipeTilemap = GetComponentInChildren<PipeTilemap>();
        public CoverTilemap CoverTilemap => _overTilemap ? _overTilemap : _overTilemap = GetComponentInChildren<CoverTilemap>();
        private WallTilemap _wallTilemap;
        private FoundationTilemap _oundationTilemap;
        private SolidTilemap _olidTilemap;
        private PipeTilemap _ipeTilemap;
        private CoverTilemap _overTilemap;

        
        
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

        public bool IsCoverHidden
        {
            set => CoverOpacity = value ? 0 : 1;
        }

        public float CoverOpacity
        {
            set
            {
                var c = CoverTilemap.Color;
                c.a = value;
                CoverTilemap.tilemap.color = c;
            }
        }
        


        public IEnumerator<BuildingTilemap> GetMapSortingOrder_BackToFront()
        {
            yield return WallTilemap;
            yield return PipeTilemap;
            yield return SolidTilemap;
            yield return FoundationTilemap;
        } 
    }

    
    
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(Building), true)]
    public class BuildingEditor : Editor
    {
        public static BuildingTilemap CreateBuildingTilemapType(Building building, TilemapType type)
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
                case TilemapType.SOLID:
                    AddCollider();
                    go.layer = LayerMask.NameToLayer("Solids");
                    go.tag = "Solid Tilemap";
                    return go.AddComponent<SolidTilemap>();
                    break;
                
                case TilemapType.WALL:
                    go.layer = LayerMask.NameToLayer("Air");
                    go.tag = "Wall";
                    return go.AddComponent<WallTilemap>();
                    break;
                
                case TilemapType.FOUNDATION:
                    go.layer = LayerMask.NameToLayer("Ground");
                    go.tag = "Solid Tilemap";
                    AddCollider();
                    return go.AddComponent<FoundationTilemap>();

                case TilemapType.PLATFORM:
                    AddCollider(asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Platforms");
                    return go.AddComponent<PlatformTilemap>();
                
                case TilemapType.PIPE:
                    AddCollider(asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Pipes");
                    go.tag = "Pipe Tilemap";
                    return go.AddComponent<PipeTilemap>();
                
                case TilemapType.COVER:
                    AddCollider(asTrigger: true);
                    go.layer = LayerMask.NameToLayer("Triggers");
                    return go.AddComponent<CoverTilemap>();

                case TilemapType.DECOR:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

      
        public override void OnInspectorGUI()
        {
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
                    if (building.SolidTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, TilemapType.SOLID));
                    if (building.WallTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, TilemapType.WALL));
                    if (building.FoundationTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, TilemapType.FOUNDATION));
                    if (building.PipeTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, TilemapType.PIPE));
                    if (building.CoverTilemap == null) AddToBuilding(CreateBuildingTilemapType(building, TilemapType.COVER));
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif
}