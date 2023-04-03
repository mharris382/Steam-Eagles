using System.IO;
using CoreLib.SaveLoad;
using UnityEngine;

namespace SaveLoad
{
    public class CharacterPositionSaver : MonoBehaviour
    {
        private void Start()
        {
            if (Directory.Exists(PersistenceManager.Instance.SaveDirectoryPath))
            {
                var airship = GameObject.FindGameObjectWithTag("Airship");
                if (airship != null)
                {
                    for (int i = 0; i < airship.transform.childCount; i++)
                    {
                        var child = airship.transform.GetChild(i);
                        if (child.CompareTag("Building"))
                        {
                            transform.parent = child;        
                        }
                    }
                }
                transform.localPosition = SpawnDatabase.Instance.LoadSpawnPointForPath(tag, PersistenceManager.Instance.SaveDirectoryPath);
            }
        }

        private void OnEnable()
        {
            PersistenceManager.Instance.GameSaved += OnSave;
            PersistenceManager.Instance.GameLoaded += OnLoad;
        }

        private void OnDisable()
        {
            PersistenceManager.Instance.GameSaved -= OnSave;
            PersistenceManager.Instance.GameLoaded -= OnLoad;
        }

        void OnSave(string path)
        {
            Debug.Log($"Saving {tag} position to path:{path}");
            SpawnDatabase.Instance.SaveSpawnPoint(tag, path, transform.localPosition);
        }

        void OnLoad(string path)
        {
            Debug.Log($"Loading {tag} Position from path: {path}");
            if (transform.parent != null)
            {
                transform.localPosition = SpawnDatabase.Instance.LoadSpawnPointForPath(tag, path);
            }
            else
            {
                transform.position = SpawnDatabase.Instance.LoadSpawnPointForPath(tag, path);    
            }
            
        }
    }
}