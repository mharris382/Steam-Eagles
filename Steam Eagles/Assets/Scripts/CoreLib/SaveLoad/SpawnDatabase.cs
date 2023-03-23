using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CoreLib.SaveLoad
{
    public class SpawnDB : ScriptableObject
{
    private static SpawnDB _instance;
 
    public static SpawnDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SpawnDB>("Spawn Database");
            }
            return _instance;
        }
    }
 
    [SerializeField]
    private List<SpawnPoint> spawnPoints;
    
    [System.Serializable]
    public class SpawnPoint
    {
        public string levelName;
        public Vector3 defaultSpawnPosition;
    }
 
    [System.Serializable]
    public class SavedSpawnPoints
    {
        public List<SavedSpawnPoint> savedSpawnPoints;
    }
    
    [System.Serializable]
    public class SavedSpawnPoint
    {
        public string levelName;
        public Vector3 spawnPosition;
    }
 
    public void UpdateSpawnPointForScene(string levelName, Vector3 position, string persistentSaveDataPath)
    {
        string filepath = Application.persistentDataPath + persistentSaveDataPath+ "/SpawnPoints.dat";
        using (FileStream file = File.Open(filepath, FileMode.Open))
        {
            var bf = new BinaryFormatter();
            SavedSpawnPoints savedSpawnPoints = (SavedSpawnPoints)bf.Deserialize(file);
            bool found = false;
            foreach (var savedSpawnPoint in savedSpawnPoints.savedSpawnPoints)
            {
                if (savedSpawnPoint.levelName == levelName)
                {
                    savedSpawnPoint.spawnPosition = spawnPoints.Find(x => x.levelName == levelName).defaultSpawnPosition;
                    found = true;
                }
            }
 
            if (!found)
            {
                savedSpawnPoints.savedSpawnPoints.Add(new SavedSpawnPoint()
                {
                    levelName = levelName,
                    spawnPosition = position
                });
            }
            bf.Serialize(file, savedSpawnPoints);
        }
    }
    
    public Vector3 GetSpawnPointForScene(string levelName, string persistentSaveDataPath)
    {
        string filepath = Application.persistentDataPath + persistentSaveDataPath + "/SpawnPoints.dat";
        using (FileStream file = File.Open(filepath, FileMode.Open))
        {
            SavedSpawnPoints savedSpawnPoints = (SavedSpawnPoints) new BinaryFormatter().Deserialize(file);
            foreach (var savedSpawnPoint in savedSpawnPoints.savedSpawnPoints)
            {
                if (savedSpawnPoint.levelName == levelName)
                {
                    return savedSpawnPoint.spawnPosition;
                }
            }
        }
        return GetDefaultSpawnPointForScene(levelName);
    }
 
    private Vector3 GetDefaultSpawnPointForScene(string sceneName)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.levelName == sceneName)
            {
                return spawnPoint.defaultSpawnPosition;
            }
        }
        Debug.LogError($"Level Name {sceneName} does not have defined spawn position, please add one", this);
        return Vector3.zero;
    }
}
}