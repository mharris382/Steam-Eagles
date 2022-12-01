using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Rand = UnityEngine.Random;
public class Spout : MonoBehaviour
{
    public SpawnableRigidbody2D spawnable;
    public ParticleSystem.MinMaxCurve spawnRate;
    
    [FormerlySerializedAs("spawner")] [SerializeField] private Internal spawner;
    public UnityEvent<Rigidbody2D> onSpawned;
    
    private float _spawnTimer;

    private void OnEnable()
    {
        StartCoroutine(nameof(Spawn));
    }
    
    private void OnDisable()
    {
        StopCoroutine(nameof(Spawn));
    }
    
    IEnumerator Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate.Evaluate(Rand.value));
            var shape = spawner.GetSpawner();
            var prefab = spawnable.GetSpawnPrefab();
            
            var spawned = Instantiate(prefab, transform.TransformPoint(shape.GetSpawnPosition()), Quaternion.identity);
            onSpawned?.Invoke(spawned);
            var waitTime = spawnRate.Evaluate(0, Rand.Range(0, 1));
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnDrawGizmos()
    {
        spawner.GetSpawner().OnDrawGizmos(this.transform);
    }

    [Serializable]
    class Internal
    {
        enum SpawnMode
        {
            CIRCLE, BOX, LINE, POINT
        }

        [SerializeField] private SpawnMode spawnMode;
        [SerializeField] private CircleSpawner circleSpawner;
        [SerializeField] private BoxSpawner boxSpawner;
        [SerializeField] private LineSpawner lineSpawner;
        [SerializeField] private PointSpawner pointSpawner;


        public Spawner GetSpawner()
        {
            switch (spawnMode)
            {
                case SpawnMode.CIRCLE:
                    return circleSpawner;
                    break;
                case SpawnMode.BOX:
                    return boxSpawner;
                    break;
                case SpawnMode.LINE:
                    return lineSpawner;
                    break;
                case SpawnMode.POINT:
                    return pointSpawner;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public abstract class Spawner
{
    [SerializeField] private ParticleSystem.MinMaxCurve spawnRotation;
    public abstract Vector3 GetSpawnPosition();

    public virtual void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        
    }
}
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

[Serializable]
public class SpawnableRigidbody2D : SpawnablePrefab<Rigidbody2D> { }

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
