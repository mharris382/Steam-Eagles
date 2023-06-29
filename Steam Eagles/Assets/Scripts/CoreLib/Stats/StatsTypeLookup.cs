using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatsTypeLookup
{
    private Dictionary<string, Type> _statTypes = new Dictionary<string, Type>();


    public bool TryGetStatType(string id, out Type type)
    {
        if (_statTypes.ContainsKey(id))
        {
            type = _statTypes[id];
            return true;
        }
        type = null;
        return false;
    }
    public Type GetStatType(string id)
    {
        if (_statTypes.ContainsKey(id))
            return _statTypes[id];
        return null;
    }
    
    public StatsTypeLookup()
    {
        var go = new GameObject("Dummy Stat manager");
        foreach (var type in GetConcreteTypes<StatBase>())
        {
            var stat = go.AddComponent(type) as StatBase;
            _statTypes.Add(stat.GetStatID(), type);
        }
        GameObject.Destroy(go);
    }
    static List<Type> GetConcreteTypes<T>()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition);

        return types.ToList();
    }
}