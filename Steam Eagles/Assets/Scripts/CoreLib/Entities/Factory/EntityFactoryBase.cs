using UnityEngine;

namespace CoreLib.MyEntities.Factory
{
    [System.Obsolete("Use EntityJsonSaveLoaderBase instead")]
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
}