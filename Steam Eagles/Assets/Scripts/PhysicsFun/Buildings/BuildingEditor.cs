#if UNITY_EDITOR

using System;
using Buildings;
using Buildings.BuildingTilemaps;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace PhysicsFun.Buildings
{
    [CustomEditor(typeof(Building), true)]
    public class BuildingEditor :
#if !ODIN_INSPECTOR
        Editor
#else
        OdinEditor
#endif
    {
        public override void OnInspectorGUI()
        {
            var building = target as Building;
           
            void CreateBuildingTilemaps(Building building1)
            {
                if (building1.SolidTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.SOLID));
                if (building1.WallTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.WALL));
                if (building1.FoundationTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.FOUNDATION));
                if (building1.PipeTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.PIPE));
                if (building1.CoverTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.COVER));
                if(building1.PlatformTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.PLATFORM));
                if(building1.WireTilemap == null) AddToBuilding(CreateBuildingTilemapType(building1, BuildingLayers.WIRES));
            }
            void AddToBuilding(BuildingTilemap buildingTilemap)
            {
                buildingTilemap.transform.parent = building.tilemapParent == null ? building.transform : building.tilemapParent;
                
            }
            
            
            serializedObject.Update();
            if (building.tilemapParent == null && GUILayout.Button("Add Tilemap Parent"))
            {
                var tilemapParent = new GameObject("[Tilemaps]");
                tilemapParent.transform.SetParent(building.transform);
                tilemapParent.transform.localPosition = Vector3.zero;
                building.tilemapParent = tilemapParent.transform;
                foreach (var allBuildingLayer in building.GetAllBuildingLayers())
                {
                    allBuildingLayer.transform.parent = tilemapParent.transform;
                }
            }
            
            if (!building.HasResources)
            {
                EditorGUILayout.HelpBox($"Building {building.name} is missing resources", MessageType.Warning);
                if (GUILayout.Button("Create Tilemaps"))
                {
                    CreateBuildingTilemaps(building);
                }
            }
            
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        public static BuildingTilemap CreateBuildingTilemapType(Building building, BuildingLayers type)
        {
            
            
            var go = new GameObject($"{building.buildingName}_{type.ToString().ToLower()}", typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(building.tilemapParent != null ? building.tilemapParent : building.transform);
            go.transform.localPosition = Vector3.zero;
            
            switch (type)
            {
                case BuildingLayers.SOLID:
                    
                    var solidTM = CreateSolidTilemap(go) as SolidTilemap;
                    var solidTMR = solidTM.GetComponent<TilemapRenderer>();
                    solidTMR.sortingOrder = solidTM.GetSortingOrder(building);
                    return solidTM;
                    break;
                
                case BuildingLayers.WALL:
                    go.layer = LayerMask.NameToLayer("Air");
                    go.tag = "Wall";
                    
                    var wallTM = go.AddComponent<WallTilemap>();
                    var tm = wallTM.Tilemap;
                    var tmr = tm.GetComponent<TilemapRenderer>();
                    tmr.sortingLayerName = "Near BG";
                    
                    return wallTM;
                    break;
                
                case BuildingLayers.FOUNDATION:
                    go.layer = LayerMask.NameToLayer("Ground");
                    go.tag = "Solid Tilemap";
                    AddCollider(go);
                    var fTM = go.AddComponent<FoundationTilemap>();
                    var fTMR = go.GetComponent<TilemapRenderer>();
                    fTMR.sortingOrder = fTM.GetSortingOrder(building);
                    return fTM;

                case BuildingLayers.PLATFORM:
                    AddCollider(go, asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Platforms");
                    var platTM = go.AddComponent<PlatformTilemap>();
                    var platTMR = go.GetComponent<TilemapRenderer>();
                    platTMR.sortingOrder = platTM.GetSortingOrder(building);
                    return platTM;
                
                case BuildingLayers.PIPE:
                    AddCollider(go, asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Pipes");
                    go.tag = "Pipe Tilemap";
                    var tmrPipe = go.GetComponent<TilemapRenderer>();
                    var pipeTM = go.AddComponent<PipeTilemap>();
                    var pipeEffector = go.GetComponent<PlatformEffector2D>();
                    pipeEffector.surfaceArc = 145;
                    tmrPipe.sortingOrder = pipeTM.GetSortingOrder(building);
                    return pipeTM;
                
                case BuildingLayers.COVER:
                    AddCollider(go, asTrigger: true);
                    go.layer = LayerMask.NameToLayer("Triggers");
                    go.AddComponent<BuildingFaderTrigger>();
                    var coverTM = go.AddComponent<CoverTilemap>();
                    var tm2 = coverTM.Tilemap;
                    var tmr2 = tm2.GetComponent<TilemapRenderer>();
                    tmr2.sortingOrder = coverTM.GetSortingOrder(building);
                    tmr2.sortingLayerName = "Near FG";
                    return coverTM;
                case BuildingLayers.WIRES:
                    AddCollider(go, asTrigger: true);
                    go.layer = LayerMask.NameToLayer("Wires");
                    var wireTM = go.AddComponent<WireTilemap>();
                    var tm3 = go.GetComponent<TilemapRenderer>();
                    tm3.sortingOrder = wireTM.GetSortingOrder(building);
                    return wireTM;

                case BuildingLayers.DECOR:
                    throw new NotImplementedException();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        private static BuildingTilemap CreateSolidTilemap(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Solids");
            go.tag = "Solid Tilemap";
            
            go.AddComponent<TilemapCollider2D>();
            return go.AddComponent<SolidTilemap>();
        }
        
        static TilemapCollider2D AddCollider(GameObject go, bool asPlatform =false, bool asTrigger =false)
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
    }
}
#endif