using System;
using UnityEngine;

[Serializable]
public class CircleSpawner : Spawner
{
    public float radius = 1;

    public override Vector3 GetSpawnPosition()
    {
        var rand = UnityEngine.Random.Range(0f, 1f);
        var angle = rand * 2 * Mathf.PI;
        var x = Mathf.Cos(angle) * radius;
        var y = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, 0);
    }

    public override  void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        var pos = position == null ? caller.transform.position : position.Value;
        Gizmos.DrawWireSphere(pos, radius);
    }
}