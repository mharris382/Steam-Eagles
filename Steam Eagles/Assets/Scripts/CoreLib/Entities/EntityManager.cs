using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CoreLib.Entities.Factory;
using SaveLoad;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities
{
    
    public class EntityManager : Singleton<EntityManager>
    {
        public override bool DestroyOnLoad => false;
        
        private string _entityPersistentSavePath =>Path.Combine(PersistenceManager.Instance.SaveDirectoryPath, "Entities");

        

        public bool debug = false;
        
        
        
        private struct EntityHandle
        {

            public GameObject Linker => Entity.linkedGameObject;
            public Entity Entity;    
            public EntityType entityType => Entity.entityType;
            public string EntityGUID => Entity.entityGUID;
            public string EntitySavePath => Path.Combine(PersistenceManager.Instance.SaveDirectoryPath, "Entities", $"{EntityGUID}.json");

            public bool doneLoading => Entity != null;
            public EntityHandle(GameObject initializer, string entityGUID, EntityType entityType)
            {
                
                var go = new GameObject($"{entityType} {entityGUID}");
                go.transform.SetParent(Instance.transform);
                Entity = go.AddComponent<Entity>();
                Entity.linkedGameObject = initializer;
                Entity.entityGUID = entityGUID;
                Entity.entityType = entityType;
                Instance.EntityFactory.CreateEntity(Entity, EntitySavePath);
                if (Instance.debug)
                    Debug.Log($"Created Entity: {entityGUID}");
                if(Instance._entityChangeListeners.ContainsKey(entityGUID))
                    Instance._entityChangeListeners[entityGUID].Value = Entity;
                MessageBroker.Default.Publish<Entity>(Entity);
            }
            
            public EntityHandle(Entity entity)
            {
                if (!Instance._entityHandles.ContainsKey(entity.entityGUID))
                {
                    Entity = entity;
                }
                else
                {
                    Debug.Assert(Instance._entityHandles[entity.entityGUID].entityType == entity.entityType, $"Requested Entity Type {entity.entityType} does not match existing entity type!");
                    Entity = Instance._entityHandles[entity.entityGUID].Entity;;    
                }
                this.Entity = entity;
                
            }
            
            public bool CanSave()
            {
                return Entity != null && Entity.linkedGameObject;
            }
            
            public void Save()
            {
                if (CanSave())
                {
                    Instance._entityFactory.SaveEntity(Entity, EntitySavePath);
                    if (Instance.debug)
                        Debug.Log($"Saved Entity: {EntityGUID}");
                }
                else
                {
                    Debug.LogError($"Cannot save entity: {EntityGUID} because it is not initialized yet!");
                }
            }
        }

        
        

        public Entity GetEntity(EntityInitializer initializer)
        {
            var guid = initializer.GetEntityGUID();
            var type = initializer.GetEntityType();
            if(_entityHandles.ContainsKey(guid)) return _entityHandles[guid].Entity;
            _entityHandles.Add(guid, new EntityHandle(initializer.gameObject, guid, type));
            return _entityHandles[guid].Entity;
        }
        [ShowInInspector]
        private Dictionary<string, EntityHandle> _entityHandles = new Dictionary<string, EntityHandle>();

        /// <summary> entities that have been registered by scene but custom data has not yet loaded from disk, so entity is not ready yet to be used. </summary>
        private Queue<Entity> _uninitializedEntities = new Queue<Entity>();
        
        
        /// <summary>
        /// allows for listening to entity changes from anywhere in the game, including persistent game elements such as UI.
        /// for example: the player inventory UI manager can listen to entity changes when the game loads, so that the new loaded
        /// state of the inventory can be updated, without reloading all the UI elements. 
        /// </summary>
        private Dictionary<string, ReactiveProperty<Entity>> _entityChangeListeners = new Dictionary<string, ReactiveProperty<Entity>>();
        
        public IReadOnlyReactiveProperty<Entity> GetEntityProperty(string entityGUID)
        {
            if (!_entityChangeListeners.ContainsKey(entityGUID))
            {
                _entityChangeListeners.Add(entityGUID, new ReactiveProperty<Entity>());
                if(_entityHandles.ContainsKey(entityGUID))
                    _entityChangeListeners[entityGUID].Value = _entityHandles[entityGUID].Entity;
            }
            return _entityChangeListeners[entityGUID];
        }
        
        private EntityFactory _entityFactory;
        private EntityFactory EntityFactory => _entityFactory ??= new EntityFactory();

        public IEnumerable<Entity> GetAllEntities()
        {
            foreach (var entityHandle in _entityHandles)
            {
                yield return entityHandle.Value.Entity;
            }
        }

        protected override void Init()
        {
            base.Init();
            var instanceSaveDirectoryPath = PersistenceManager.Instance.SaveDirectoryPath;
            if (!string.IsNullOrEmpty(instanceSaveDirectoryPath)  && Directory.Exists(instanceSaveDirectoryPath))
            {
                OnSavePathChanged(instanceSaveDirectoryPath);
            }

            if(_entityFactory == null)
                _entityFactory = new EntityFactory();
            PersistenceManager.Instance.GameSaved += SaveAllEntities;
        }

        private void OnSavePathChanged(string instanceSaveDirectoryPath)
        {
            //_entityPersistentSavePath = Path.Combine(instanceSaveDirectoryPath, "Entities");
            if (!Directory.Exists(_entityPersistentSavePath)) 
                Directory.CreateDirectory(_entityPersistentSavePath);
        }


        //private void OnDestroy()
        //{
        //    SaveAllEntities();
        //}

        private void SaveAllEntities(string path)
        {
            foreach (var entityHandle in _entityHandles)
            {
                if(entityHandle.Value.CanSave())
                    entityHandle.Value.Save();
                else
                {
                    //Debug.Log($"Failed to save entity: {entityHandle.Key}");
                }
            }
            
        }

        public void LoadDynamicEntity(string entityEntityGUID, EntityType entityEntityType)
        {
            if (_entityHandles.ContainsKey(entityEntityGUID))
            {
                Debug.Assert(_entityHandles[entityEntityGUID].entityType == entityEntityType, $"Requested Entity Type {entityEntityType} does not match existing entity type!");
                return;
            }
            //TODO: need to create a new dynamic entity which requires spawning a new entity and resolving a prefab for the entity
            // var entityHandle = new EntityHandle(entityEntityGUID, entityEntityType);
            // _entityHandles.Add(entityEntityGUID, entityHandle);
            throw new NotImplementedException();
        }

        public void SaveEntity(string getEntityGUID)
        {
            if(_entityHandles.ContainsKey(getEntityGUID))
                _entityHandles[getEntityGUID].Save();
        }
    }

    [System.Serializable]
    public class DynamicEntitiesSaveData : IEnumerable<DynamicEntitiesSaveData.DynamicEntitySaveData>
    {
        [SerializeField]
        private List<DynamicEntitySaveData> dynamicEntities = new List<DynamicEntitySaveData>();
        
        [System.Serializable]
        public class DynamicEntitySaveData
        {
            public string entityGUID;
            public EntityType entityType;
        }

        public void AddEntity(string entityGUID, EntityType entityType)
        {
            dynamicEntities.Add(new DynamicEntitySaveData()
            {
                entityGUID = entityGUID,
                entityType = entityType
            });
        }

        public IEnumerator<DynamicEntitySaveData> GetEnumerator()
        {
            return dynamicEntities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dynamicEntities).GetEnumerator();
        }
    }
}