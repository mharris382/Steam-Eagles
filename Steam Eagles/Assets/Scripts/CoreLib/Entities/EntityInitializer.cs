using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities
{
    public abstract class EntityInitializer : MonoBehaviour
    {
        public abstract string GetEntityGUID();
        public abstract EntityType GetEntityType();
        
        private ReactiveProperty<Entity> _entity = new ReactiveProperty<Entity>();
        
        public Entity Entity => _entity.Value;
        public IReadOnlyReactiveProperty<Entity> EntityProperty => _entity;

        public bool isDoneInitializing;

        /// <summary>
        /// once entity has been setup to store correct custom data (such as inventory data) and all
        /// save data has been loaded, this method is called to all implementing classes to then
        /// pass the data to the user once it is ready to be used
        /// </summary>
        /// <param name="entity"></param>
        public abstract void OnEntityInitialized(Entity entity);

        private void Awake()
        {
            isDoneInitializing = false;
        }

        private void OnEnable()
        {
            if (!isDoneInitializing)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            Debug.Log($"Initializing Entity {GetEntityGUID()} ({GetEntityType()})",this);
            EntityManager.Instance.StartCoroutine(WaitForEntityToLoad(GetEntityGUID(), GetEntityType()));
        }

        // private IEnumerator Start()
        // {
        //     isDoneInitializing = false;
        //     while(EntityManager.Instance == null)
        //         yield return null;
        //     
        //     var entityGUID = GetEntityGUID();
        //     var entityType = GetEntityType();
        //     
        //     //yield return UniTask.ToCoroutine(async () =>
        //     //{
        //     //    if(EntityManager.Instance.debug) Debug.Log($"Initializing Entity: {entityGUID} ({entityType})");
        //     //    var result = await EntityManager.Instance.GetEntityAsync(this);
        //     //    if (EntityManager.Instance.debug) Debug.Log($"Finished Initializing Entity: {entityGUID} ({entityType})");
        //     //    _entity.Value = result;
        //     //    OnEntityInitialized(result);
        //     //    isDoneInitializing = true;
        //     //});
        //    
        //     //OnEntityInitialized(_entity.Value = EntityManager.Instance.GetEntity(this));
        // }
        //

        IEnumerator WaitForEntityToLoad(string entityGUID, EntityType entityType)
        {
            if(EntityManager.Instance.debug) Debug.Log($"Initializing Entity: {entityGUID} ({entityType})",this);
            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await EntityManager.Instance.GetEntityAsync(this);
                if (EntityManager.Instance.debug)
                    Debug.Log($"Finished Initializing Entity: {entityGUID} ({entityType})");
                _entity.Value = result;
                OnEntityInitialized(result);
                isDoneInitializing = true;
                MessageBroker.Default.Publish(new EntityInitializedInfo(this));
            });
        }
        
        private void OnDisable()
        {
            if(EntityManager.SafeInstance != null)
            {
                EntityManager.SafeInstance.SaveEntity(GetEntityGUID());
                EntityManager.SafeInstance.UnloadEntity(GetEntityGUID());
            }
            _entity.Dispose();
        }
    }
    
    
    public struct EntityInitializedInfo
    {
        public readonly string entityGUID;
        public readonly Entity entity;
        public readonly GameObject linkedGameObject;

        public EntityInitializedInfo(string entityGUID, Entity entity)
        {
            this.entityGUID = entityGUID;
            this.entity = entity;
            linkedGameObject = null;
        }

        internal EntityInitializedInfo(EntityInitializer initializer)
        {
            entityGUID = initializer.GetEntityGUID();
            entity = initializer.Entity;
            linkedGameObject = initializer.gameObject;
        }
    }
}