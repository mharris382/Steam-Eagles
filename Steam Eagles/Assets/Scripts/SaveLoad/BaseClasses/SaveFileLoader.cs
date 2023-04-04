using System;
using System.IO;
using UnityEngine;

namespace SaveLoad
{
    public abstract class SaveFileLoader<T> : IGameLoader
    {

        /// <summary>
        /// attempt to load the save state from the given path, return true if successful
        /// </summary>
        /// <param name="savePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool LoadSaveState(string savePath)
        {
            if (!File.Exists(GetSavePath(savePath)))
            {
                Debug.LogError($"Path not found: {savePath}");
                return false;
            }
            var json = File.ReadAllText(GetSavePath(savePath));
            var data = JsonUtility.FromJson<T>(json);
            return LoadData(data);
        }

        public abstract bool LoadData(T data);
        
        string GetSavePath(string path) => $"{path}\\{typeof(T).Name}.json";
        public void LoadGame(string savePath)
        {
            LoadSaveState(savePath);
        }
    }
}