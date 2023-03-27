using System;
using System.Linq;
using Buildings;
using Buildings.BuildingTilemaps;
using UnityEngine;
using System.IO;
using CoreLib;
using Sirenix.OdinInspector;

namespace PhysicsFun.Buildings
{
    [Serializable]
    public class BuildingData
    {
        
        public LevelTilemapData[] tilemaps;
        
        public BuildingData(Building building)
        {
            tilemaps = building.GetAllBuildingLayers().Select(x => new LevelTilemapData(x)).ToArray();
        }
    }
    
    [RequireComponent(typeof(Building))]
    public class BuildingSaveLoader : MonoBehaviour
    {
        private Building _building;
        public Building Building => _building ? _building : _building = GetComponent<Building>();

        [SerializeField] private string customSavePath = "/CustomSaves";
        private string GetPath() => $"{Application.persistentDataPath}{customSavePath}/{ Building.buildingName}.json";

        [Button("Save")]
        public void SaveBuilding()
        {
            var buildingName = Building.buildingName;
            var buildingData = new BuildingData(Building);
            string json = JsonUtility.ToJson(buildingData, true);
            File.WriteAllText(GetPath(), json);
            
            Debug.Log($"Saved building: {buildingName}\n" +
                      $"{GetPath()}"  );
        }
        [Button("Load")]
        public void LoadBuilding()
        {
            var buildingName = Building.buildingName;
            string json = File.ReadAllText(GetPath());
            BuildingData buildingData = JsonUtility.FromJson<BuildingData>(json);
            if (buildingData == null)
            {
                Debug.LogError($"Building data at {GetPath()} is null");
                return;
            }
            var tilemaps = Building.GetAllBuildingLayers().ToArray();
            Debug.Assert(tilemaps != null);
            Debug.Assert(buildingData != null);
            Debug.Assert(buildingData.tilemaps != null);
            Debug.Assert(tilemaps.Length == buildingData.tilemaps.Length, "Tilemap count mismatch", this);
            for (int i = 0; i < buildingData.tilemaps.Length; i++)
            {
                var buildingLayer = tilemaps[i];
                var data = buildingData.tilemaps[i];
                for (int j = 0; j < data.positions.Count; j++)
                {
                    buildingLayer.Tilemap.SetTile(data.positions[i], data.tiles[i]);
                }
            }
            Debug.Log($"Loaded building: {buildingName.Bolded()}\n" +
                      $"{GetPath().Bolded()}"  );
        }
        
        public class BuildingSaver
        {
            
            private readonly Building _building;
            private readonly string _id;
            private readonly BuildingTilemap[] _tilemaps;

            public BuildingSaver(Building building)
            {
                _building = building;
                _id = building.buildingName;
                _tilemaps = building.GetAllBuildingLayers().ToArray();
            }

            public void Save(string path = "")
            {
                // var buildingData = new BuildingData(_id, _tilemaps);
                // var json = JsonUtility.ToJson(buildingData);
                // File.WriteAllText(path, json);
                string[] saveData = new string[_tilemaps.Length];
                for (int i = 0; i < _tilemaps.Length; i++)
                {
                    var tm = _tilemaps[i];
                    
                }
            }

            
        }
        
        public class BuildingLoader
        {
            private readonly Building _building;
            private readonly string _id;
            private readonly BuildingTilemap[] _tilemaps;

            public BuildingLoader(Building building)
            {
                _building = building;
                _id = building.buildingName;
                _tilemaps = building.GetAllBuildingLayers().ToArray();
            } 
        }
    }
}