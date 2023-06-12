using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities
{
    [Serializable]
    public class EntityCoreSaveData
    {
        [SerializeField]
        public List<RegisteredEntities> registeredEntities;
        public EntityCoreSaveData(EntityLinkRegistry linkRegistry)
        {
            Dictionary<EntityType, RegisteredEntities> registeredEntitiesDict = new Dictionary<EntityType, RegisteredEntities>();
            foreach (var allEntity in linkRegistry.GetAllEntities())
            {
                var type = allEntity.EntityType;
                if (!registeredEntitiesDict.ContainsKey(type))
                {
                    registeredEntitiesDict.Add(type, new RegisteredEntities(type));
                }
                registeredEntitiesDict[type].entityGuids.Add(allEntity.GUID);
            }
        }
            
            
        public class Factory : PlaceholderFactory<EntityCoreSaveData> { }


        [Serializable]
        public class RegisteredEntities
        {
            [SerializeField]
            public EntityType type;
            [SerializeField]
            public List<string> entityGuids;
            public RegisteredEntities(EntityType type)
            {
                type = type;
                entityGuids = new List<string>();
            }
                
        }
    }
}