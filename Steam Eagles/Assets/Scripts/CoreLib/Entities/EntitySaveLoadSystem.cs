using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SaveLoad;

namespace CoreLib.Entities
{
    /// <summary>
    /// this class hooks up the entity save system into the global save system as a save loader system
    /// <see cref=""/>
    /// </summary>
    [UsedImplicitly]
    [LoadOrder(10000)]
    public class EntitySaveLoadSystem : ISaveLoaderSystem
    {
        private readonly EntityLinkRegistry _entityLinkRegistry;
        private readonly EntityHandle.Factory _entityHandleFactory;

        public EntitySaveLoadSystem(EntityLinkRegistry entityLinkRegistry, EntityHandle.Factory entityHandleFactory, DynamicEntityCoreSaveData.Factory entityCoreSaveDataFactory)
        {
            _entityLinkRegistry = entityLinkRegistry;
            _entityHandleFactory = entityHandleFactory;
        }

        public string SubFolderName() => "Entities";

        public async UniTask<bool> SaveGameAsync(string savePath)
        {
            var entityHandles = _entityLinkRegistry.Values.Select(t => _entityHandleFactory.Create(t)).ToList();
            var results = await UniTask.WhenAll(Enumerable.Select(entityHandles, t => t.Save()));
            return Enumerable.All<bool>(results, t => t);
        }

        public async UniTask<bool> LoadGameAsync(string loadPath)
        {
            var entityHandles = _entityLinkRegistry.Values.Select(t => _entityHandleFactory.Create(t)).ToList();
            var results = await UniTask.WhenAll(Enumerable.Select(entityHandles, t => t.Load()));
            foreach (var entityHandle in entityHandles)
            {
                entityHandle.LinkedGameObject.GetComponent<EntityInitializer>().Initialize();
            }
            return Enumerable.All<bool>(results, t => t);
        }

        public IEnumerable<(string name, string ext)> GetSaveFileNames()
        {
            yield break;
            //yield return (nameof(DynamicEntityCoreSaveData), "json");
        }
    }
}