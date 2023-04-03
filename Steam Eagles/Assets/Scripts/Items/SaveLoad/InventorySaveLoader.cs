using CoreLib.SaveLoad;
using UnityEngine;
using System.IO;
using SaveLoad;

namespace Items.SaveLoad
{
    public static class InventorySaveLoader 
    {
        private static InventorySaves _loadedSaves;

        /// <summary>
        /// lazy loaded, loads on first request then caches the result in memory. Cache can be cleared by calling 
        /// <see cref="UnloadInventorySave"/>
        /// </summary>
        public static InventorySaves LoadedInventorySave
        {
            get
            {
                if (_loadedSaves == null)
                {
                    LoadInventorySave();
                    if (_loadedSaves == null) 
                        _loadedSaves = new InventorySaves();
                }
                return _loadedSaves;
            }
        }
       
        /// <summary>
        /// this will load inventory saves provided they have not already been loaded.  If you want to reload the
        /// inventories and revert changes that have been made, you need to first call <see cref="UnloadInventorySave"/>
        /// before calling this
        /// </summary>
        private static void LoadInventorySave()
        {
            if(_loadedSaves != null)
                return;
            string filepath = $"{Application.persistentDataPath}/{PersistenceManager.SavePath}/InventorySaves.json";
            
            if (File.Exists(filepath))
            {
                string json = File.ReadAllText(filepath);
                _loadedSaves = JsonUtility.FromJson<InventorySaves>(json);
            }
            else
            {
                Debug.LogWarning($"No inventory saves found at: {filepath}");
                _loadedSaves = new InventorySaves();
            }
        }

        /// <summary>
        /// will overwrite inventory saves at the current save path
        /// </summary>
        public static void SaveInventoryState()
        {
            if(_loadedSaves == null)
                return;
            string filepath = $"{Application.persistentDataPath}/{PersistenceManager.SavePath}/InventorySaves.json";
            SaveInventorySave(filepath);
        }
        
        private static void SaveInventorySave(string path)
        {
            if (LoadedInventorySave.Count == 0) Debug.LogWarning($"Saving empty inventory state at: {path}");
            string json = JsonUtility.ToJson(LoadedInventorySave);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// note this will erase any changes that have been made and not saved
        /// </summary>
        public static void UnloadInventorySave()
        {
            _loadedSaves = null;
        }
    }
}