using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveLoad.Entities
{
    [System.Serializable]
    public class EntityStatsSaveData
    {
        public List<EntityStats> entityStats;

        public EntityStatsSaveData()
        {
            entityStats = new();
        }

        public EntityStatsSaveData(Dictionary<string, List<StatValues>> entityStatValues)
        {
            entityStats = new();
            foreach (var (key, value) in entityStatValues)
            {
                if (string.IsNullOrEmpty(key) || value == null || value.Count == 0) continue;
                entityStats.Add(new EntityStats(key, value));
            }
        }


        public List<StatValues> GetStatValues(string entityGUID)
        {
            var stat = entityStats.FirstOrDefault(e => e.entityGUID == entityGUID);
            if (stat == null)
            {
                return null;
            }
            else
            {
               return stat.stats;
            }
        }
        public void UpdateStatValues(string entityGUID, List<StatValues> values)
        {
            var stat = entityStats.FirstOrDefault(e => e.entityGUID == entityGUID);
            if (stat == null)
            {
                stat = new EntityStats(entityGUID, values);
            }
            else
            {
                stat.stats = values;
            }
        }

        [System.Serializable]
        public class EntityStats
        {
            public string entityGUID;
            public List<StatValues> stats;

            public EntityStats(string guid, List<StatValues> stats)
            {
                this.entityGUID = guid;
                this.stats = stats;
            }
        }
        public Dictionary<string, List<StatValues>> GetEntityStatLookup()
        {
            var dict = new Dictionary<string, List<StatValues>>();
            foreach (var stats in entityStats)
            {
                if(!string.IsNullOrEmpty(stats.entityGUID) || stats.stats == null) continue;
                Debug.Assert(!dict.ContainsKey(stats.entityGUID), $"Saved multiple stats for the same entity! {stats.entityGUID}");
                if(dict.ContainsKey(stats.entityGUID))continue; 
                dict.Add(stats.entityGUID, stats.stats);
            }
            return dict;
        }
    }
}