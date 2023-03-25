using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace CoreLib.SaveLoad
{
    [CreateAssetMenu(menuName = "Spawn DB")]
    public class SpawnDatabase : ScriptableObject
{
    private static SpawnDatabase _instance;
 
    public static SpawnDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SpawnDatabase>("Spawn Database");
            }
            return _instance;
        }
    }
 
    [SerializeField]
    private List<SpawnPoint> spawnPoints;
    
    private Dictionary<string, Transform> _dynamicSpawnPoints = new Dictionary<string, Transform>();

    [System.Serializable]
    public class SpawnPoint
    {
        public string characterName;
        public string spawnScene;
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
        public string characterName;
        public string spawnScene;
        public Vector3 spawnPosition;
    }
 
    public Vector3 GetDefaultSpawnPointForScene(string characterName)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                return spawnPoint.defaultSpawnPosition;
            }
        }
        Debug.LogError($"Level Name {characterName} does not have defined spawn position, please add one", this);
        return Vector3.zero;
    }

    
    public void RegisterDynamicSpawn(string character, Transform spawnPoint)
    {
        if (_dynamicSpawnPoints.ContainsKey(character))
        {
            _dynamicSpawnPoints[character] = spawnPoint;
        }
        else
        {
            _dynamicSpawnPoints.Add(character, spawnPoint);
        }
    }
    
    public void RemoveDynamicSpawn(string character)
    {
        if (_dynamicSpawnPoints.ContainsKey(character))
        {
            _dynamicSpawnPoints.Remove(character);
        }
    }
    
    public Vector3 GetSpawnPointForScene(string characterName, string persistentSaveDataPath)
    {
        Vector3 spawnPoint;
        
        if (_dynamicSpawnPoints.ContainsKey(characterName))
        {
            if(_dynamicSpawnPoints[characterName] != null)
                return _dynamicSpawnPoints[characterName].position;
            else
                _dynamicSpawnPoints.Remove(characterName);
        }
        
        if(TryLoadSpawnPoint(characterName, persistentSaveDataPath, out spawnPoint))
            return spawnPoint;
        
        return GetDefaultSpawnPointForScene(characterName);
    }
    
    private bool TryLoadSpawnPoint(string characterName, string persistentSaveDataPath, out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;
        string dirPath = Application.persistentDataPath + persistentSaveDataPath;

        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.dat" : "/SpawnPoints.dat");
        
        using (FileStream file = (File.Exists(filepath) ? File.Open(filepath, FileMode.Open) : File.Create(filepath)))
        {
            SavedSpawnPoints savedSpawnPoints = (SavedSpawnPoints) new BinaryFormatter().Deserialize(file);
            foreach (var savedSpawnPoint in savedSpawnPoints.savedSpawnPoints)
            {
                if (savedSpawnPoint.characterName == characterName)
                {
                    spawnPoint = savedSpawnPoint.spawnPosition;
                    return true;
                }
            }
        }
        return false;
    }

    private SavedSpawnPoints LoadSpawnPoints()
    {
        
        throw new NotImplementedException();
    }


    public bool HasDefaultSpawnPosition(string characterName)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                return true;
            }
        }
        return false;
    }
    public void UpdateDefaultSpawnPoint(string characterName, Vector3 spawnPosition)
    {
        if (!Application.isEditor || Application.isPlaying)
        {
            Debug.LogError("Default Spawn points can only be called in editor", this);
            return;
        }
        
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                spawnPoint.defaultSpawnPosition = spawnPosition;
                return;
            }
        }
        Debug.Log("Adding new spawn point for character " + characterName, this);
        spawnPoints.Add(new SpawnPoint()
        {
            characterName = characterName,
            defaultSpawnPosition = spawnPosition
        });
    }
}

    public class SpawnDBHelper : MonoBehaviour
    {
        public SpawnDatabase spawnDatabase;

        private void OnDrawGizmos()
        {
            if (spawnDatabase == null) spawnDatabase = SpawnDatabase.Instance;
        }
    }
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(SpawnDatabase))]
    public class SpawnDBHelperEditor : OdinEditor
    {
        
    }
    #endif
}