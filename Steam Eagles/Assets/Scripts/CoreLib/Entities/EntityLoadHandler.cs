using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities
{
    /// <summary>
    /// currently this class automatically triggers entity loading when entity is added to the registry <see cref="EntityLinkRegistry"/>
    /// this behavior needs to be changed as it forces the entity to be loaded, however in the case where the entity has never been saved
    /// this will cause an error.  We need to confirm that entity has been saved before attempting to load it.
    /// uses the <see cref="IEntitySaveLoader"/> to perform save/load operations
    /// </summary>
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
           // _linkRegistry.OnValueAdded.Select(t => (t, _handleFactory.Create(t)))
           //     .Subscribe(tup =>
           //     {
           //         _entities.Add(tup.t, tup.Item2);
           //         LoadEntity(tup.t, tup.Item2);
           //     });
           // _linkRegistry.OnValueRemoved.Subscribe(OnEntityLinkDestroyed);
        }

        void OnEntityLinkDestroyed(EntityInitializer initializer)
        {
            if (_entityLoadOps.TryGetValue(initializer, out var op))
            {
                if(_coroutineCaller != null && op != null)
                    _coroutineCaller.StopCoroutine(op);
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
}