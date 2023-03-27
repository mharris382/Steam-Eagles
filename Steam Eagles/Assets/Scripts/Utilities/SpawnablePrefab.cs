using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class SpawnablePrefab<T> where  T: Component
{
    [SerializeField] private bool spawnFromCollection;
    
    [DisableIf("spawnFromCollection")]
    [SerializeField] private  T prefab;
    
    [ShowIf("spawnFromCollection")]
    [SerializeField] private List<T> prefabCollection;

    public T GetSpawnPrefab()
    {
        
        if (!spawnFromCollection || prefabCollection == null || prefabCollection.Count == 0) return prefab;
        throw new NotImplementedException();
    }
}