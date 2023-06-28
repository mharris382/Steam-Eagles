using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace CoreLib.Entities
{
    public class EntityTrackingInfo
    {
        private readonly EntityInitializer _initializer;
        private readonly ReactiveProperty<Component> _lastSeenBuilding;
        private readonly ReactiveProperty<Component> _lastSeenRoom;


        public Component LastSeenBuilding
        {
            get => _lastSeenBuilding.Value;
            set => _lastSeenBuilding.Value = value;
        }
        
        public Component LastSeenRoom
        {
            get => _lastSeenRoom.Value;
            set => _lastSeenRoom.Value = value;
        }
        
        public IReadOnlyReactiveProperty<Component> LastSeenBuildingProperty => _lastSeenBuilding;
        public IReadOnlyReactiveProperty<Component> LastSeenRoomProperty => _lastSeenRoom;
        

        public EntityTrackingInfo(EntityInitializer initializer)
        {
            _initializer = initializer;
            _lastSeenBuilding = new ReactiveProperty<Component>();
            _lastSeenRoom = new ReactiveProperty<Component>();
        }
    }
    
    [RequireComponent(typeof(StatManager))]
    public abstract class EntityInitializer : MonoBehaviour
    {
        public abstract string GetEntityGUID();
        public abstract EntityType GetEntityType();
        
        private ReactiveProperty<Entity> _entity = new ReactiveProperty<Entity>();
        
        public Entity Entity => _entity.Value;
        public IReadOnlyReactiveProperty<Entity> EntityProperty => _entity;

        public bool isDoneInitializing;
        private EntityLinkRegistry linkRegistry;
        private EntityTrackingInfo _trackingInfo;
        private CoroutineCaller coroutineCaller;
        private EntityConfig logger;

        public UnityEvent<GameObject> onEntityLoaded;

        public EntityTrackingInfo TrackingInfo => _trackingInfo ??= new EntityTrackingInfo(this);
        
        

        [Inject]
        void Inject(EntityLinkRegistry linkRegistry, CoroutineCaller coroutineCaller, EntityConfig globalConfig)
        {
            this.logger = globalConfig;
            this.coroutineCaller = coroutineCaller;
            this.linkRegistry = linkRegistry;
            bool result =this.linkRegistry.Register(this);
            Debug.Assert(result, $"Failed to register Entity {GetEntityGUID()} ({GetEntityType()})");
            Debug.Log($"Registered Entity {GetEntityGUID()} ({GetEntityType()})",this);
        }
       
        /// <summary>
        /// once entity has been setup to store correct custom data (such as inventory data) and all
        /// save data has been loaded, this method is called to all implementing classes to then
        /// pass the data to the user once it is ready to be used
        /// </summary>
        /// <param name="entity"></param>
        public abstract void OnEntityInitialized(Entity entity);

        public abstract bool IsReadyToLoad();

        private void Awake()
        {
            isDoneInitializing = false;
        }

 

        private void OnDestroy()
        {
            if(linkRegistry != null)
                linkRegistry.Unregister(this);
        }

        public void Initialize()
        {
            Debug.Log($"Initialized Entity {GetEntityGUID()} ({GetEntityType()})",this);
            isDoneInitializing = true;
            onEntityLoaded?.Invoke(gameObject);
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

    public abstract class SubEntityInitializer : EntityInitializer
    {
        
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