using System;
using System.Collections;
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