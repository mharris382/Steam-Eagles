using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SaveLoad;
using UnityEngine;
using Zenject;

namespace CoreLib.Entities
{
    public class EntitySaveLoadInstaller : Installer<EntitySaveLoadInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<EntitySavePath>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EntityRegistry>().AsSingle().NonLazy();
            Container.BindFactory<EntityInitializer, EntityHandle, EntityHandle.Factory>().AsSingle().NonLazy();
            Container.Bind(typeof(IInitializable), typeof(IEntitySaveLoader)).To<EntitySaveLoader>().AsSingle().NonLazy();
            var entityTypeSaveLoaders = ReflectionUtils.GetConcreteTypes<IEntityTypeSaveLoader>();
            foreach (var entityTypeSaveLoaderType in entityTypeSaveLoaders)
            {
                Container.Bind<IEntityTypeSaveLoader>().To(entityTypeSaveLoaderType).AsSingle().NonLazy();
            }

            Container.BindFactory<EntityCoreSaveData, EntityCoreSaveData.Factory>().AsSingle().NonLazy();
        }

     

        class EntitySaveLoader : IEntitySaveLoader, IInitializable
        {
            private readonly EntityConfig _config;
            private readonly Dictionary<EntityType, List<IEntityTypeSaveLoader>> _entityTypeSaveLoaders;
            private readonly List<IEntityTypeSaveLoader> _entitySaveLoaders;

            public EntitySaveLoader(EntityConfig config, List<IEntityTypeSaveLoader> entityTypeSaveLoaders)
            {
                _config = config;
                _entitySaveLoaders = entityTypeSaveLoaders;
                _entityTypeSaveLoaders = new Dictionary<EntityType, List<IEntityTypeSaveLoader>>();
            }
            
            public async UniTask<bool> SaveEntity(EntityHandle handle)
            {
                if (!CanSaveOrLoad(handle))
                {
                    return false;
                }
                var saveLoader = _entityTypeSaveLoaders[handle.Type];
                var results = await UniTask.WhenAll(saveLoader.Select(t => t.SaveEntity(handle)));
                bool allSucceeded = true;
                for (int i = 0; i < results.Length; i++)
                {
                    if (!results[i])
                    {
                        allSucceeded = false;
                        Debug.LogError($"Failed to save entity: {handle.LinkedGameObject.name} with loader: {saveLoader[i].GetType().Name}");
                    }
                }
                return allSucceeded;
            }
            public async UniTask<bool> LoadEntity(EntityHandle handle)
            {
                if (!CanSaveOrLoad(handle))
                {
                    return false;
                }
                var saveLoader = _entityTypeSaveLoaders[handle.Type];
                var results = await UniTask.WhenAll(saveLoader.Select(t => t.LoadEntity(handle)));
                bool allSucceeded = true;
                for (int i = 0; i < results.Length; i++)
                {
                    if (!results[i])
                    {
                        allSucceeded = false;
                        Debug.LogError($"Failed to load entity: {handle.LinkedGameObject.name} with loader: {saveLoader[i].GetType().Name}");
                    }
                }
                return allSucceeded;
            }

            private bool CanSaveOrLoad(EntityHandle handle)
            {
                var type = handle.Type;
                if (!_entityTypeSaveLoaders.ContainsKey(type))
                {
                    Debug.LogError($"No save loaders for entity type {type}");
                    return false;
                }
                var guid = handle.EntityGUID;
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"Entity {handle.LinkedGameObject.name} has no GUID");
                    return false;
                }
                return true;
            }
            public void Initialize()
            {
                foreach (var entityTypeSaveLoader in _entitySaveLoaders)
                {
                    var type = entityTypeSaveLoader.GetEntityType();
                    if (!_entityTypeSaveLoaders.ContainsKey(type))
                    {
                        _entityTypeSaveLoaders.Add(type, new List<IEntityTypeSaveLoader>());
                    }
                    _entityTypeSaveLoaders[type].Add(entityTypeSaveLoader);
                }
            }
        }


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
        
        
        [UsedImplicitly]
        public class EntitySaveLoadSystem : ISaveLoaderSystem
        {
            private readonly EntitySavePath _savePath;
            private readonly EntityLinkRegistry _entityLinkRegistry;
            private readonly EntityHandle.Factory _entityHandleFactory;

            public EntitySaveLoadSystem(EntitySavePath savePath,
                EntityLinkRegistry entityLinkRegistry, 
                EntityHandle.Factory entityHandleFactory, EntityCoreSaveData.Factory entityCoreSaveDataFactory)
            {
                _savePath = savePath;
                _entityLinkRegistry = entityLinkRegistry;
                _entityHandleFactory = entityHandleFactory;
            }
            
            public async UniTask<bool> SaveGameAsync(string savePath)
            {
                var entityHandles = _entityLinkRegistry.Values.Select(t => _entityHandleFactory.Create(t)).ToList();
                var results = await UniTask.WhenAll(entityHandles.Select(t => t.Save()));
                return results.All(t => t);
            }

            public async UniTask<bool> LoadGameAsync(string loadPath)
            {
                var entityHandles = _entityLinkRegistry.Values.Select(t => _entityHandleFactory.Create(t)).ToList();
                var results = await UniTask.WhenAll(entityHandles.Select(t => t.Save()));
                return results.All(t => t);
            }

            public IEnumerable<(string name, string ext)> GetSaveFileNames()
            {
                yield break;
                //yield return (nameof(EntityCoreSaveData), "json");
            }
        }
        
    }
}