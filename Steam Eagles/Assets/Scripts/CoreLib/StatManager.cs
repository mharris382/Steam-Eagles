using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class StatManager : MonoBehaviour
{
    private Dictionary<string,StatBase> stats = new ();
    private StatsTypeLookup _typeLookup;
    public IEnumerable<StatBase> Stats => stats.Values;

    [Inject] private void InjectMe(StatsTypeLookup typeLookup)
    {
        _typeLookup = typeLookup;
    }
    
    public bool AddStat(StatBase stat)
    {
        if (stat == null) return false;
        var id = stat.GetStatID();
        if (stats.ContainsKey(id)) return false;
        stats.Add(id,stat);
        return true;
    }
    public bool RemoveStat(StatBase stat)
    {
        if (stat == null) return false;
        var id = stat.GetStatID();
        return stats.Remove(id);
    }
    public bool HasStat(string id) => stats.ContainsKey(id) && stats[id] != null;
    public StatBase GetStat(string id)
    {
        if (!HasStat(id)) return null;
        return stats[id];
    }
    
    public List<StatValues> GetStatValues() => Stats.Select(t => t.GetValues()).ToList();

    public void SetStatValues(List<StatValues> statValues)
    {
        foreach (var statValue in statValues)
        {
            if (!HasStat(statValue.id))
            {
                if(!_typeLookup.TryGetStatType(statValue.id, out var type)) continue;
                var stat = gameObject.AddComponent(type) as StatBase;
                if (stat == null) continue;
                AddStat(stat);
            }
            stats[statValue.id].SetValues(statValue);
        }
    }

    
}

[Serializable]
public struct StatValues
{
    public string id;
    public int maxValue;
    public int currentValue;
}