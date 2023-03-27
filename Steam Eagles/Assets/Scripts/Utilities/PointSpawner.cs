using System;
using UnityEngine;

[Serializable]
public class PointSpawner : Spawner
{
    public Transform spawnPoint;
    public override Vector3 GetSpawnPosition()
    {
        return spawnPoint.position;
    }
    
    public override void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position ?? spawnPoint.position, 0.1f);
    }
}