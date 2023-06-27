using System;
using System.Collections.Generic;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;


namespace SaveLoad.Entities
{
    [UsedImplicitly]
    [LoadOrder(-1000)]
    public class EntityStatSaveDataLoader : AsyncSaveFileLoader<EntityStatsSaveData>, ISaveLoaderSystem
    {
        private readonly EntityLinkRegistry _linkRegistry;
        private Dictionary<string, List<StatValues>> _statValues;
        
        
        public EntityStatSaveDataLoader(EntityLinkRegistry linkRegistry)
        {
            _linkRegistry = linkRegistry;
        }
        public override UniTask<bool> LoadData(string savePath, EntityStatsSaveData data)
        {
            _statValues = data.GetEntityStatLookup();
            return UniTask.FromResult(true);
        }

        public override UniTask<EntityStatsSaveData> GetSaveData(string savePath)
        {
            if(_statValues == null) _statValues = new Dictionary<string, List<StatValues>>();
            return UniTask.FromResult(new EntityStatsSaveData(_statValues));
        }

        public override bool IsSystemOptional() => true;

        public string SubFolderName()
        {
            return "";
        }
        public override IEnumerable<(string name, string ext)> GetSaveFileNames()
        {
            yield return ("EntityStats", "json");
        }
    }
}