using System;
using UnityEngine;

[Serializable]
public class LineSpawner : Spawner
{
    public Vector3 lineStart;
    public Vector3 lineEnd;

    public override Vector3 GetSpawnPosition()
    {
        var p0 = lineStart;
        var p1 = lineEnd;
        var rand = UnityEngine.Random.Range(0f, 1f);
        return Vector3.Lerp(p0, p1, rand);
    }

    public override  void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        var p0 = caller.TransformPoint(lineStart);
        var p1 = caller.TransformPoint(lineEnd);
        Gizmos.DrawLine(p0, p1);
    }
}