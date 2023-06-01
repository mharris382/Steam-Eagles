using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities
{
    [InfoBox("Bound at Project Context")]
    public class EntityGlobalInstaller : MonoInstaller
    {
        [SerializeField] private EntityConfig config = new EntityConfig() { };
        public override void InstallBindings()
        {
            Container.Bind<EntityConfig>().AsSingle().NonLazy();
            Container.BindInterfacesTo<EntityLoadHandler>().AsSingle().NonLazy();
            Container.Bind<EntityLinkRegistry>().AsSingle().NonLazy();
            Container.Bind<Registry<EntityHandle>>().To<EntityHandle.Registry>().AsSingle().NonLazy();
            EntitySaveLoadInstaller.Install(Container);
        }

       
    }
    [Serializable]
    public class EntityConfig : ConfigBase { }


    public class EntityLoadHandler : IInitializable
    {
        private readonly EntityHandle.Factory _handleFactory;
        private readonly EntityLinkRegistry _linkRegistry;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly IEntitySaveLoader _saveLoader;
        private readonly Registry<EntityHandle> _entityRegistry;


        private Dictionary<EntityInitializer, EntityHandle> _entities = new Dictionary<EntityInitializer, EntityHandle>();
        private Dictionary<EntityInitializer, Coroutine> _entityLoadOps = new Dictionary<EntityInitializer, Coroutine>();

        public EntityLoadHandler(IEntitySaveLoader saveLoader, 
            EntityHandle.Factory handleFactory,
            EntityLinkRegistry linkRegistry,
            CoroutineCaller coroutineCaller)
        {
            _handleFactory = handleFactory;
            _linkRegistry = linkRegistry;
            _coroutineCaller = coroutineCaller;
            _saveLoader = saveLoader;
        }

        public void Initialize()
        {
            _linkRegistry.OnValueAdded.Select(t => (t, _handleFactory.Create(t)))
                .Subscribe(tup =>
                {
                    _entities.Add(tup.t, tup.Item2);
                    LoadEntity(tup.t, tup.Item2);
                });
            _linkRegistry.OnValueRemoved.Subscribe(OnEntityLinkDestroyed);
        }

        void OnEntityLinkDestroyed(EntityInitializer initializer)
        {
            if (_entityLoadOps.ContainsKey(initializer))
            {
                var loadOp = _entityLoadOps[initializer];
                if(_coroutineCaller != null)
                    _coroutineCaller.StopCoroutine(loadOp);
            }
            if (_entities.ContainsKey(initializer))
            {
                var handle = _entities[initializer];
            }
        }
        void LoadEntity(EntityInitializer initializer, EntityHandle entityHandle)
        {
            _entityLoadOps.Add(initializer, _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var result = await _saveLoader.LoadEntity(entityHandle);
                if (!result) Debug.LogError($"Failed to load entity: {entityHandle.LinkedGameObject.name}");
                initializer.isDoneInitializing = true;
            })));
        }
    
    }
    

    public interface IEntitySaveLoader
    {
        UniTask<bool> SaveEntity(EntityHandle handle);
        UniTask<bool> LoadEntity(EntityHandle handle);
    }

    public interface IEntityTypeSaveLoader : IEntitySaveLoader
    {
        public EntityType GetEntityType();
        
    }
}