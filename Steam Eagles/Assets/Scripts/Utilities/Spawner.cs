using UnityEngine;

public abstract class Spawner
{
    [SerializeField] private ParticleSystem.MinMaxCurve spawnRotation;
    public abstract Vector3 GetSpawnPosition();

    public virtual void OnDrawGizmos(Transform caller, Vector3? position = null)
    {
        
    }
}