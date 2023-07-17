using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }
    
    public void OnDestroy(ref SystemState state)
    {
        
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var spawnerRef in SystemAPI.Query<RefRW<Spawner>>())
        {
            ProcessSpawner(ref state, spawnerRef);
        }
    }

    private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawnerRef)
    {
        var spawner = spawnerRef.ValueRO;
        if (spawner.SpawnCount == 0) return;
        spawner.SpawnCount--;
        if (spawner.NextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            Entity newEntity = state.EntityManager.Instantiate(spawner.Prefab);
            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));
            spawner.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.SpawnRate;
        }
    }
}
