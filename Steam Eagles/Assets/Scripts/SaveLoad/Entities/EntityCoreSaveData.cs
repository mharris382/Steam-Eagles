using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.MyEntities;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaveLoad.Entities
{
    
    [System.Serializable]
    public class EntityCoreSaveData
    {
        [FormerlySerializedAs("entityStats")] public List<EntityData> entityData;
        private Dictionary<string, EntityData> _entityDataDict;
        public EntityCoreSaveData()
        {
            entityData = new();
        }
        public EntityCoreSaveData(List<EntityData> entityDataValues)
        {
            entityData = new();
            _entityDataDict = new();
            
            void Add(EntityData ed)
            {
                if (!ed.IsValid()) return;
                if (_entityDataDict.ContainsKey(ed.entityGUID)) return;
                entityDataValues.Add(ed);
                _entityDataDict.Add(ed.entityGUID, ed);
            }
            
            foreach (var data in entityDataValues)
            {
                Add(data);
            }
        }
        [Obsolete]
        public EntityCoreSaveData(Dictionary<string, List<StatValues>> entityStatValues)
        {
            entityData = new();
            foreach (var (key, value) in entityStatValues)
            {
                if (string.IsNullOrEmpty(key) || value == null || value.Count == 0) continue;
                entityData.Add(new EntityData(key, value));
            }
        }


        public List<StatValues> GetStatValues(string entityGUID)
        {
            var stat = entityData.FirstOrDefault(e => e.entityGUID == entityGUID);
            if (stat == null)
            {
                return null;
            }
            else
            {
               return stat.stats;
            }
        }
        

        [System.Serializable]
        public class EntityData
        {
            public string entityGUID;
            public List<StatValues> stats;

            public EntityData(string guid, List<StatValues> stats)
            {
                this.entityGUID = guid;
                this.stats = stats;
            }

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(entityGUID);
            }

            public static implicit operator EntityData(EntityInitializer entityInitializer)
            {
                var statManager = entityInitializer.GetComponent<StatManager>();
                Debug.Assert(statManager != null, $"Missing Stat Manager on {entityInitializer.name}",entityInitializer);
                return new EntityData(entityInitializer.GetEntityGUID(), statManager.GetStatValues());
            }
            
            
            public void Load(EntityInitializer entityInitializer)
            {
                var statManager = entityInitializer.GetComponent<StatManager>();
                Debug.Assert(statManager != null, $"Missing Stat Manager on {entityInitializer.name}",entityInitializer);
                statManager.SetStatValues(stats);
            }
        }
        public Dictionary<string, EntityData> GetEntityStatLookup()
        {
            var dict = new Dictionary<string, EntityData>();
            foreach (var stats in entityData)
            {
                if(!string.IsNullOrEmpty(stats.entityGUID) || stats.stats == null) continue;
                Debug.Assert(!dict.ContainsKey(stats.entityGUID), $"Saved multiple stats for the same entity! {stats.entityGUID}");
                if(dict.ContainsKey(stats.entityGUID))continue; 
                dict.Add(stats.entityGUID, stats);
            }
            return dict;
        }
    }
}