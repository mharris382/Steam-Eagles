using System;
using UnityEngine;

[Serializable]
public class BoxSpawner : Spawner
{
    public Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 size = new Vector3(1, 1, 1);

    public override Vector3 GetSpawnPosition()
    {
        var x = UnityEngine.Random.Range(-size.x / 2, size.x / 2);
        var y = UnityEngine.Random.Range(-size.y / 2, size.y / 2);
        var z = UnityEngine.Random.Range(-size.z / 2, size.z / 2);
        var offsetWorld = new Vector3(offset.x *  size.x, offset.y * size.y, offset.z * size.z);
        return new Vector3(x, y, z) + (offsetWorld);
    }

    public override void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        var pos = position == null ? caller.transform.position : position.Value;
        Gizmos.DrawWireCube(pos, size);
    }
}