using System;
using System.Collections.Generic;
using System.IO;
using SaveLoad;
using UnityEngine;

namespace CoreLib.Entities.Factory
{
    public class EntityFactory
    {
        private Dictionary<EntityType, EntityFactoryBase> _entityFactories;
        public EntityFactory()
        {
            _entityFactories = new Dictionary<EntityType, EntityFactoryBase>();
            var factories = ReflectionUtils.GetConcreteInstances<EntityFactoryBase>();
            foreach (var entityFactory in factories)
            {
                _entityFactories.Add(entityFactory.GetEntityType(), entityFactory);
            }
        }
        public void CreateEntity(Entity entity, string path)
        {
            var entityType = entity.entityType;
            var factory = GetFactoryFor(entityType);
            factory.LoadEntityFromJSON(entity, path);
            if (factory == null)
            {
                throw new EntityTypeFactoryNotImplemented(entityType);
            }

            
        }

        
        public  EntityFactoryBase GetFactoryFor(EntityType entityType)
        {
            if (_entityFactories.TryGetValue(entityType, out var result))
            {
                return result;
            }
            
            _entityFactories.Add(entityType, result);
            return result;
        }

        public void SaveEntity(Entity entity, string entitySavePath)
        {
            var entityType = entity.entityType;
            var factory = GetFactoryFor(entityType);
            if (factory == null) throw new EntityTypeFactoryNotImplemented(entityType);
            factory.SaveEntityToJSON(entity, entitySavePath);
        }
    }
}