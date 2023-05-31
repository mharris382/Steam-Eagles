using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.IO;
namespace CoreLib.Entities.Factory
{
    public abstract class EntityFactoryBase<T> : EntityFactoryBase
    {
        protected override void LoadFromJSON(Entity entity, string json)
        {
            var data = JsonUtility.FromJson<T>(json);
            if (data == null)
            {
                Debug.LogWarning($"Failed to load entity from JSON. Entity: {entity.entityGUID}\nJSON: {json}");
                return;
            }
            LoadFromSaveData(entity, data);
        }

        protected override string SaveToJSON(Entity entity)
        {
            var saveData = GetSaveDataFromEntity(entity);
            if (EntityManager.Instance.debug) 
                Debug.Log($"Saving Entity: {entity.entityGUID}");
            
            var json =  JsonUtility.ToJson(saveData, EntityManager.Instance.debug);
            
            if (EntityManager.Instance.debug)
                Debug.Log($"Save Data for Entity: {entity.entityGUID}\n{json.InItalics()}");
            
            return json;
        }

        protected abstract void LoadFromSaveData(Entity entity, T data);

        protected abstract T GetSaveDataFromEntity(Entity entity);
    }

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
            var jsonFilePath = $"{_savePath.FullSaveDirectoryPath}/{guid}.json";
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
            var jsonFilePath = $"{_savePath.FullSaveDirectoryPath}/{guid}.json";
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