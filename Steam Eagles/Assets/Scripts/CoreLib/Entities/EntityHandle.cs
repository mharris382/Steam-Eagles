using System;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities
{
    
    public class EntityHandle : IDisposable
    {
        private readonly Registry<EntityHandle> _entityHandle;

        public class Registry : Registry<EntityHandle> { }
        
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
        public EntityHandle(EntityInitializer initializer, Registry<EntityHandle> entityHandle) : this(initializer.gameObject, initializer.GetEntityGUID(),
            initializer.GetEntityType())
        {
            _entityHandle = entityHandle;
            _entityHandle.Register(this);
        }

        public bool CanEntityBeSaved()
        {
            if (!_savePath.HasSavePath)
                return false;
            return LinkedGameObject != null;
        }

        public void Dispose()
        {
            _entityHandle.Unregister(this);
        }
    }
}