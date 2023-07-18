using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.MyEntities;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UniRx;
using Zenject;


namespace SaveLoad.Entities
{
    [UsedImplicitly]
    [LoadOrder(-1000)]
    public class EntityStatSaveDataLoader : AsyncSaveFileLoader<EntityCoreSaveData>, ISaveLoaderSystem, IInitializable, IDisposable
    {
        private readonly EntityLinkRegistry _linkRegistry;
        private Dictionary<string, EntityCoreSaveData.EntityData> _statValues;
        private CompositeDisposable _cd = new();
        
        public EntityStatSaveDataLoader(EntityLinkRegistry linkRegistry)
        {
            _linkRegistry = linkRegistry;
        }
        public override UniTask<bool> LoadData(string savePath, EntityCoreSaveData data)
        {
            _statValues = data.GetEntityStatLookup();
            return UniTask.FromResult(true);
        }

        public override UniTask<EntityCoreSaveData> GetSaveData(string savePath)
        {
            foreach (var entity in _linkRegistry.Values) UpdateEntityData(entity);
            return UniTask.FromResult(new EntityCoreSaveData(_statValues.Values.ToList()));
        }

        public override bool IsSystemOptional() => true;

        public string SubFolderName()
        {
            return "";
        }
        public override IEnumerable<(string name, string ext)> GetSaveFileNames()
        {
            yield return ("EntityData", "json");
        }

        private bool IsLoaded() => _statValues != null;
        
        public void Initialize()
        {
            _linkRegistry.OnValueAdded.Where(_ => _ != null && IsLoaded()).Subscribe(LoadEntity).AddTo(_cd);
            _linkRegistry.OnValueRemoved.Where(_ => _ != null && IsLoaded()).Subscribe(UpdateEntityData).AddTo(_cd);
        }

        void UpdateEntityData(EntityInitializer entityInitializer)
        {
            if(entityInitializer == null) return;
            var entityGuid = entityInitializer.GetEntityGUID();
            if (!_statValues.ContainsKey(entityGuid))
            {
                _statValues.Add(entityGuid,entityInitializer);
            }
            else
            {
                _statValues[entityGuid] = entityInitializer;
            }
        }
        void LoadEntity(EntityInitializer entityInitializer)
        {
            var entityGuid = entityInitializer.GetEntityGUID();
            if (!_statValues.ContainsKey(entityGuid)) UpdateEntityData(entityInitializer);
            else _statValues[entityGuid].Load(entityInitializer);
        }
        
        public void Dispose()
        {
            _cd.Dispose();
        }
    }
}