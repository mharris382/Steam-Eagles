using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities.Factory
{
    public abstract class EntityJsonSaveLoaderBase<T> : IEntityTypeSaveLoader 
    {
        private EntitySavePath _savePath;

        [Inject]
        public void InjectSavePath(EntitySavePath savePath) => _savePath = savePath;

        public abstract EntityType GetEntityType();
        public async UniTask<bool> SaveEntity(EntityHandle handle)
        {
            if (_savePath == null)
                return false;
            var guid = handle.EntityGUID;
            var jsonFilePath =Path.Combine(_savePath.FullSaveDirectoryPath, $"{guid}.json");
            var saveData =  await GetDataFromEntity(handle);
            if (saveData == null)
            {
                Debug.LogError("Failed to get save data from entity");
                return false;
            }

            var json = JsonUtility.ToJson(saveData);
            try
            {
                await File.WriteAllTextAsync(jsonFilePath, json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public async UniTask<bool> LoadEntity(EntityHandle handle)
        {
            if (_savePath == null || !_savePath.HasSavePath)
                return false;
            var guid = handle.EntityGUID;
            var jsonFilePath = Path.Combine(_savePath.FullSaveDirectoryPath, $"{guid}.json");
            if (!File.Exists(jsonFilePath))
            {
                Debug.LogError($"Failed to load entity. No save file found at {jsonFilePath}");
                return false;
            }
            var json = await File.ReadAllTextAsync(jsonFilePath);
            try
            {
                var data = JsonUtility.FromJson<T>(json);
                if (data == null)
                {
                    Debug.LogError($"Failed to load entity from JSON. Entity: {guid}\nJSON: {json}\n{jsonFilePath}");
                    return false;
                }
                return await LoadDataIntoEntity(handle, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load entity from JSON. Entity: {guid}\nJSON: {json}\n{jsonFilePath}");
                return false;
            }
            return true;
        }
        
        public abstract UniTask<T> GetDataFromEntity(EntityHandle entityHandle);
        public abstract UniTask<bool> LoadDataIntoEntity(EntityHandle entityHandle, T data);
    }
}