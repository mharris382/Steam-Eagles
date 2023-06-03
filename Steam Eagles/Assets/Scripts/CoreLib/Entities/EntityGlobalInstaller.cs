using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SaveLoad;
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
            Container.BindFactory<EntityInitializer, EntityV2, EntityV2.Factory>().AsSingle().NonLazy();
            Container.Bind<EntitySaveHandler>().AsSingle().NonLazy();
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
            Debug.Log("Initializing EntityLoadHandler");
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
                if(_coroutineCaller != null && loadOp != null)
                    _coroutineCaller.StopCoroutine(loadOp);
            }
            if (_entities.ContainsKey(initializer))
            {
                var handle = _entities[initializer];
            }
        }
        void LoadEntity(EntityInitializer initializer, EntityHandle entityHandle)
        {
            _entityLoadOps.Add(initializer, 
                _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var result = await _saveLoader.LoadEntity(entityHandle);
                if (!result) Debug.LogError($"Failed to load entity: {entityHandle.LinkedGameObject.name}, SaveLoader:{_saveLoader.GetType().Name}");
                initializer.Initialize();
            })));
        }

        public async UniTask<bool> LoadEntityAsync(EntityInitializer initializer, EntityHandle entityHandle)
        {
            var result = await _saveLoader.LoadEntity(entityHandle);
            if (!result) Debug.LogError($"Failed to load entity: {entityHandle.LinkedGameObject.name}");
            initializer.Initialize();
            return result;
        }
        
        public async UniTask<bool> SaveGameAsync(string savePath)
        {
            Debug.Log($"Saving Entities: Count= {_entityRegistry.Values.Count()}");
            var results = await UniTask.WhenAll(_entityRegistry.Values.Select(t => t.Save()));
            return results.All(t => t);
        }

        public async UniTask<bool> LoadGameAsync(string loadPath)
        {
            Debug.Log($"Loading Entities: Count= {_entityRegistry.Values.Count()}");
            var results = await UniTask.WhenAll(_entityRegistry.Values.Select(t => t.Load()));
            return results.All(t => t);
        }

        public IEnumerable<(string name, string ext)> GetSaveFileNames()
        {
            yield break;
        }
    }


    public class EntitySaveHandler
    {
        private readonly EntityLinkRegistry _linkRegistry;
        private readonly EntityHandle.Factory _entityHandleFactory;

        public EntitySaveHandler(EntityLinkRegistry linkRegistry, EntityHandle.Factory entityHandleFactory)
        {
            _linkRegistry = linkRegistry;
            _entityHandleFactory = entityHandleFactory;
        }

        public async UniTask<bool> LoadEntities()
        {
            Debug.Log($"Saving Entities: Count={_linkRegistry.ValueCount}");
            var results = await UniTask.WhenAll(_linkRegistry.Values.Select(t => _entityHandleFactory.Create(t).Load()));
            return results.All(t => t);
        }
        public async UniTask<bool> SaveEntities()
        {
            Debug.Log($"Saving Entities: Count={_linkRegistry.ValueCount}");
            var results = await UniTask.WhenAll(_linkRegistry.Values.Select(t => _entityHandleFactory.Create(t).Save()));
            return results.All(t => t);
        }
    }
}