using System;
using System.IO;
using Buildings.BuildingTilemaps;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Buildings.SaveLoad
{
    [RequireComponent(typeof(BuildingTilemap))]
    public class BuildingTilemapSaveLoader : MonoBehaviour
    {
        BuildingTilemap   _tilemap;
        private BuildingTilemap tilemap => _tilemap ? _tilemap : _tilemap = GetComponent<BuildingTilemap>();

        
        public SaveSlot GetSaveSlot()
        {
            var slot = SaveLoadHandler.Instance.GetCurrentSaveSlot();
            return slot;
        }
        
        [Button]
        void SaveLevel()
        {
            
            var buildingTilemapData = new LevelTilemapData(tilemap);
            var json = JsonUtility.ToJson(buildingTilemapData);
            File.WriteAllText($"{Application.persistentDataPath}/{name}.json", json);
            
        }
        [Button]
        void LoadLevel()
        {
            string json = File.ReadAllText($"{Application.persistentDataPath}/{name}.json");
            LevelTilemapData buildingTilemapData = JsonUtility.FromJson<LevelTilemapData>(json);
            tilemap.Tilemap.ClearAllTiles();
            for (int i = 0; i < buildingTilemapData.positions.Count; i++)
            {
                var pos = buildingTilemapData.positions[i];
                var tile = buildingTilemapData.tiles[i];
                tilemap.Tilemap.SetTile(pos, tile);
            }
        }
    }
}