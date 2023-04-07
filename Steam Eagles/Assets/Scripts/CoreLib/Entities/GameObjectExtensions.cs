using System.Collections.Generic;
using Statuses;
using UnityEngine;

namespace CoreLib.EntityTag
{
    public static class GameObjectExtensions
    {
        public static Dictionary<GameObject, Entity> s_EntityCache = new Dictionary<GameObject, Entity>();
        public static Dictionary<GameObject, Entity> s_remoteEntityLinks = new Dictionary<GameObject, Entity>();
        

        private static GameObject s_entitiesRoot;
        private static GameObject EntitiesRoot
        {
            get
            {
                if (s_entitiesRoot == null)
                {
                    s_entitiesRoot = new GameObject("[Entities]");
                    s_entitiesRoot.transform.position = Vector3.zero;
                    //GameObject.DontDestroyOnLoad(s_entitiesRoot);
                }

                return s_entitiesRoot;
            }
        }

        public static Entity GetEntity(this GameObject gameObject)
        {
            if (s_remoteEntityLinks.TryGetValue(gameObject, out var e))
            {
                if(e !=null)
                    return e;
                Debug.LogWarning("Found link to null entity, removing link", gameObject);
                s_remoteEntityLinks.Remove(gameObject);
            }
            
            if (s_EntityCache.TryGetValue(gameObject, out var entity))
            {
                if(entity != null)
                    return entity;
                Debug.LogWarning("Found null entity in cache, clearing cached entry", gameObject);
            }
            
            entity = gameObject.GetComponent<Entity>();
            if (entity == null)
            {
                entity = gameObject.GetComponentInParent<Entity>();
            }
            s_EntityCache.Add(gameObject, entity);
            
            return entity;
        }
        
        
        /// <summary>
        /// does not create new objects, but associates the GameObject with the given entity, provided
        /// that GameObject is not already associated with another entity
        /// </summary>
        /// <param name="gameObject">gameObject that will be associated with the entity <see cref="GetEntity"/></param>
        /// <param name="linkedEntity">entity that will be associated with the gameObject  <see cref="GetEntity"/></param>
        /// <returns></returns>
        public static bool RegisterRemoteEntityLink(this GameObject gameObject, Entity linkedEntity)
        {
            if (gameObject.TryGetEntity(out var existingEntity))
            {
                Debug.Log($"This GameObject ({gameObject.name}) is already associated with entity: {existingEntity.name}");
                return false;
            }
            s_remoteEntityLinks.Add(gameObject, linkedEntity);
            return true;
        }

        public static Entity CreateEntityLinked(this GameObject gameObject, string guid=null)
        {
            if (gameObject.TryGetEntity(out var existingEntity))
            {
                Debug.Log($"This GameObject ({gameObject.name}) is already associated with entity: {existingEntity.name}");
                return existingEntity;
            }

            if (guid == null || string.IsNullOrEmpty(guid))
            {
                guid = System.Guid.NewGuid().ToString();
            }
            var entityGO = new GameObject("Entity", typeof(Entity));
            var entity = entityGO.GetComponent<Entity>();
            entity.entityGUID = guid;
            
            entityGO.transform.SetParent(EntitiesRoot.transform);
            s_EntityCache.Add(entityGO, entity);
            RegisterRemoteEntityLink(gameObject, entity);
            return entity;
        }
        
        public static Entity CreateEntityAttached(this GameObject gameObject)
        {
            
            if (gameObject.TryGetEntity(out var existingEntity))
            {
                Debug.Log($"This GameObject ({gameObject.name}) is already associated with entity: {existingEntity.name}");
                return existingEntity;
            }
            var entity = gameObject.GetComponent<Entity>();
            gameObject.transform.SetParent(EntitiesRoot.transform);
            s_EntityCache.Add(gameObject, entity);
            RegisterRemoteEntityLink(gameObject, entity);
            return entity;
        }

        public static bool TryGetEntity(this GameObject go, out Entity entity)
        {
            var existingEntity = go.GetEntity();
            if (existingEntity != null)
            {
                entity = existingEntity;
                return true;
            }
            entity = null;
            return false;
        }
    }
}