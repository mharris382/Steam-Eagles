using System.IO;
using UnityEngine;

namespace CoreLib.Entities.Factory
{
    public abstract class EntityFactoryBase
    {
        public void SaveEntityToJSON(Entity entity, string savePath)
        {
            savePath = savePath.Replace('\\', '/');
            var json = SaveToJSON(entity);
            File.WriteAllText(savePath, json);
        }
        public void LoadEntityFromJSON(Entity entity, string savePath)
        {
            savePath = savePath.Replace('\\', '/');
            if (!File.Exists(savePath))
            {
                if (EntityManager.Instance.debug)
                    Debug.Log($"No save file found for entity: {entity.entityGUID} at path: {savePath}.");
                return;
            }
            var json = File.ReadAllText(savePath);
            LoadFromJSON(entity, json);
        }

        protected abstract void LoadFromJSON(Entity entity, string json);
        protected abstract string SaveToJSON(Entity entity);
        
        public abstract EntityType GetEntityType();
    }
}