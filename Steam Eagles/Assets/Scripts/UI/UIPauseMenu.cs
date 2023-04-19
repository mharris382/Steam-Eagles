using System;
using System.Collections.Generic;
using CoreLib;
using CoreLib.SharedVariables;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace UI
{
    public class UIPauseMenu : MonoBehaviour
    {
        public SharedBool isPaused;
        [Header("Settings")]
        [SerializeField] private bool pauseTime = true;

        public SharedTransform tpTransform;
        public SharedTransform tpSpawnTransform;
        public SharedTransform bdTransform;
        public SharedTransform bdSpawnTransform;

        private void Awake()
        {
            isPaused.Value = false;
            isPaused.onValueChanged.AsObservable().Where(x => x).Subscribe(_ => Pause()).AddTo(this);
            isPaused.onValueChanged.AsObservable().Where(x => !x).Subscribe(_ => Resume()).AddTo(this);
        }

        private void Pause()
        {
            if(pauseTime)
                Time.timeScale = 0;
        }
        
        private void Resume()
        {
            if(pauseTime)
                Time.timeScale = 1;
        }

        public void TogglePause()
        {
            isPaused.Value = !isPaused.Value;
            if (pauseTime)
            {
                Time.timeScale = isPaused.Value ? 0 : 1;
            }
        }
        
        public void PauseGame()
        {
            isPaused .Value = true;
        }
        
        public void ResumeGame()
        {
            isPaused.Value = false;
        }


        public void RespawnTP()
        {
            if(!tpTransform.HasValue || !tpSpawnTransform.HasValue)
                return;
            tpTransform.Value.position = tpSpawnTransform.Value.position;
        }

        public void RespawnBD()
        {
            if (bdTransform.HasValue == false || bdSpawnTransform.HasValue == false)
                return;
            bdTransform.Value.position = bdSpawnTransform.Value.position;
        }
        
        public void RespawnBoth()
        {
            RespawnBD();
            RespawnTP();
        }
        public void RestartScene()
        {
            RespawnBoth();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    
        public void QuitToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
        
        public void QuitButton()
        {
            Application.Quit();   
        }
        
        
    }
    
    public enum UIPauseMenuState
    {
        None,
        Pause,
        Settings
    }
}

public class PlayerSpawnPosition : MonoBehaviour
{
    
}
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