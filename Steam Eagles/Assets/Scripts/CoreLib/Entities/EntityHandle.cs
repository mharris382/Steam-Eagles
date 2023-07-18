using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CoreLib.MyEntities
{
    
    public class EntityHandle
    {
        private readonly IEntitySaveLoader _saveLoader;
        private GlobalSavePath _savePath;
        
        public GameObject LinkedGameObject { get; }
        public string EntityGUID { get; }
        public EntityType Type { get; }

        public class Factory : PlaceholderFactory<EntityInitializer, EntityHandle>{}

       
        
        EntityHandle(GameObject linkedGameObject, string entityGUID, EntityType type)
        {
            LinkedGameObject = linkedGameObject;
            EntityGUID = entityGUID;
            Type = type;
        }
        
        [Inject]
        public EntityHandle(EntityInitializer initializer,
            IEntitySaveLoader saveLoader) : this(initializer.gameObject, initializer.GetEntityGUID(),
            initializer.GetEntityType())
        {
            _saveLoader = saveLoader;
        }

        public bool CanEntityBeSaved()
        {
            if (!_savePath.HasSavePath)
                return false;
            return LinkedGameObject != null;
        }

        
        public UniTask<bool> Save() => _saveLoader.SaveEntity(this);
        public UniTask<bool> Load() => _saveLoader.LoadEntity(this);
    }
}