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
            }
            void AddToBuilding(BuildingTilemap buildingTilemap)
            {
                buildingTilemap.transform.parent = building.transform;
                
            }
            
            
            serializedObject.Update();

            
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
            
            var go = new GameObject($"{building}_{type.ToString().ToLower()}", typeof(Tilemap), typeof(TilemapRenderer));

            
            switch (type)
            {
                case BuildingLayers.SOLID:
                    
                    return CreateSolidTilemap(go);
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
                    return go.AddComponent<FoundationTilemap>();

                case BuildingLayers.PLATFORM:
                    AddCollider(go, asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Platforms");
                    return go.AddComponent<PlatformTilemap>();
                
                case BuildingLayers.PIPE:
                    AddCollider(go, asPlatform: true);
                    go.layer = LayerMask.NameToLayer("Pipes");
                    go.tag = "Pipe Tilemap";
                    return go.AddComponent<PipeTilemap>();
                
                case BuildingLayers.COVER:
                    AddCollider(go, asTrigger: true);
                    go.layer = LayerMask.NameToLayer("Triggers");
                    go.AddComponent<BuildingFaderTrigger>();
                    var coverTM = go.AddComponent<CoverTilemap>();
                    var tm2 = coverTM.Tilemap;
                    var tmr2 = tm2.GetComponent<TilemapRenderer>();
                    tmr2.sortingLayerName = "Near FG";
                    return coverTM;
                

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