using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SaveLoad
{
    
    public class SpawnDatabase : ScriptableObject
{
    private static SpawnDatabase _instance;

    public static SpawnDatabase Instance => _instance ??= Resources.Load<SpawnDatabase>("Spawn Database");

    [SerializeField]
    private List<SpawnPoint> spawnPoints;
    
    private Dictionary<string, Transform> _dynamicSpawnPoints = new Dictionary<string, Transform>();

    [System.Serializable]
    public class SpawnPoint
    {
        public string characterName;
        public Vector3 defaultSpawnPosition;
    }
 
    [System.Serializable]
    public class SavedSpawnPoints
    {
        [SerializeField]
        public SavedSpawnPoint[] savedSpawnPoints = new SavedSpawnPoint[0];


        public void CreateOrUpdate(string characterName, Vector3 spawnPosition)
        {
            int cnt = 0;
            foreach (var savedSpawnPoint in savedSpawnPoints)
            {
                if (savedSpawnPoint.characterName == characterName)
                {
                    savedSpawnPoint.spawnPosition = spawnPosition;
                    savedSpawnPoints[cnt] = savedSpawnPoint;
                    Debug.Log($"Updated spawn point for character: {characterName} to {spawnPosition}");
                    return;
                }
                cnt++;
            }

            var los = new List<SavedSpawnPoint>(savedSpawnPoints);
            Debug.Log($"Created spawn point for character: {characterName} at {spawnPosition}");
            los.Add(new SavedSpawnPoint()
            {
                characterName = characterName,
                spawnPosition = spawnPosition
            });
            savedSpawnPoints = los.ToArray();
        }

        public bool GetSavePointFor(string character, out Vector3 point)
        {
            point = Vector3.zero;
            foreach (var savedSpawnPoint in savedSpawnPoints)
            {
                if (savedSpawnPoint.characterName == character)
                {
                    point = savedSpawnPoint.spawnPosition;
                    return true;
                }
            }

            return false;
        }
    }
    
    [System.Serializable]
    public class SavedSpawnPoint
    {
        public string characterName;
        public Vector3 spawnPosition;
    }
 
    public Vector3 GetDefaultSpawnPointForCharacter(string characterName)
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
    
    [Obsolete("prefer async version. GetSpawnPoint")]
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
        
        return GetDefaultSpawnPointForCharacter(characterName);
    }

    public async UniTask<Vector3> GetSpawnPoint(string characterName)
    {
        if (_dynamicSpawnPoints.ContainsKey(characterName))
        {
            if(_dynamicSpawnPoints[characterName] != null)
                return _dynamicSpawnPoints[characterName].position;
            else
                _dynamicSpawnPoints.Remove(characterName);
        }

        if (string.IsNullOrEmpty(PersistenceManager.Instance.SaveDirectoryPath))
        {
            Debug.LogWarning("Save directory path is null, waiting for it to be set");
            await new WaitUntil(() => !string.IsNullOrEmpty(PersistenceManager.Instance.SaveDirectoryPath));
        }
        
        string filePath = Path.Combine(PersistenceManager.Instance.SaveDirectoryPath, "SpawnPoints.json");
        if (!File.Exists(filePath))
        {
            return GetDefaultSpawnPointForCharacter(characterName);
        }

        var json = await File.ReadAllTextAsync(filePath);
        var spawnPoints = JsonUtility.FromJson<SavedSpawnPoints>(json);
        if (spawnPoints == null)
        {
            return GetDefaultSpawnPointForCharacter(characterName);
        }
        if(spawnPoints.GetSavePointFor(characterName, out var pnt))
        {
            return pnt;
        };
        return GetDefaultSpawnPointForCharacter(characterName);
    }
    
    
    
    private bool TryLoadSpawnPoint(string characterName, string persistentSaveDataPath, out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;
        string dirPath = persistentSaveDataPath.StartsWith(Application.persistentDataPath) ? persistentSaveDataPath : $"{Application.persistentDataPath}/{persistentSaveDataPath}";

        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.json" : "/SpawnPoints.json");
        if (!File.Exists(filepath))
        {
            return false;
        }

        var spawnSaves = LoadSpawnPoints(filepath);
        foreach (var savedSpawnPoint in spawnSaves.savedSpawnPoints)
        {
            if (savedSpawnPoint.characterName == characterName)
            {
                spawnPoint = savedSpawnPoint.spawnPosition;
                return true;
            }
        }
        
        return false;
    }

    public Vector3 LoadSpawnPointForPath(string characterName, string savePath)
    {
        string filepath =  savePath + (savePath.EndsWith("/") ? "SpawnPoints.json" : "/SpawnPoints.json");
        var spawnPoints = LoadSpawnPoints(filepath);
        if (spawnPoints.GetSavePointFor(characterName, out var point))
        {
            return point;
        }
        return GetDefaultSpawnPointForCharacter(characterName);
    }
    public void SaveSpawnPoint(string characterName, string dirPath, Vector3 spawnPoint)
    {
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.json" : "/SpawnPoints.json");
        var sp = LoadSpawnPoints(filepath);
        sp.CreateOrUpdate(characterName, spawnPoint);
        var json = JsonUtility.ToJson(sp);
        File.WriteAllText(filepath, json);
        Debug.Log($"Saved spawn point for {characterName} at path {filepath}");
    }

    public async UniTask SaveSpawnPointAsync(string characterName, string dirPath, Vector3 spawnPoint)
    {
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.json" : "/SpawnPoints.json");
        var sp = await LoadSpawnPointsAsync(filepath);
        sp.CreateOrUpdate(characterName, spawnPoint);
        var json = JsonUtility.ToJson(sp);
        await File.WriteAllTextAsync(filepath, json);
        Debug.Log($"Saved spawn point for {characterName} at path {filepath}");
    }
    public async UniTask SaveSpawnPointsAsync(string dirPath, IEnumerable<(string, Vector3)> spawnPoints)
    {
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.json" : "/SpawnPoints.json");
        var sp = await LoadSpawnPointsAsync(filepath);
        foreach (var spawnPoint in spawnPoints)
        {
            sp.CreateOrUpdate(spawnPoint.Item1, spawnPoint.Item2);
        }
        var json = JsonUtility.ToJson(sp);
        await File.WriteAllTextAsync(filepath, json);
    }
    public async UniTask<SavedSpawnPoints> LoadSpawnPointsAsync(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No File exists at {path}");
            return new SavedSpawnPoints();
        }
        string json = await File.ReadAllTextAsync(path);
        return JsonUtility.FromJson<SavedSpawnPoints>(json);
    }
    SavedSpawnPoints LoadSpawnPoints(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No File exists at {path}");
            return new SavedSpawnPoints();
        }
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SavedSpawnPoints>(json);
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
    
}