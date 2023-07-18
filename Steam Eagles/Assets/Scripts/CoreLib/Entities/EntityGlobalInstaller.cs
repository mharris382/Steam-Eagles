using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using SaveLoad;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace CoreLib.MyEntities
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
            Container.Bind<EntityLoadSignalBus>().AsSingle().NonLazy();
            EntitySaveLoadInstaller.Install(Container);
        }

       
    }
    [Serializable]
    public class EntityConfig : ConfigBase { }


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