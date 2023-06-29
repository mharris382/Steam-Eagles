using System;
using UnityEngine;
using Zenject;

namespace CoreLib
{
    public class SpawnPoint : MonoBehaviour
    {
        private SpawnPoints _spawnPoints;

        [Inject] void Inject(SpawnPoints spawnPoints)
       {
           _spawnPoints = spawnPoints;
           _spawnPoints.Register(this);
       }

        private void OnEnable()
        {
            if(_spawnPoints != null)
                _spawnPoints.Register(this);
        }

        private void OnDisable()
        {
            if(_spawnPoints != null)
                _spawnPoints.Unregister(this);
        }
    }

    public class SpawnPoints : Registry<SpawnPoint>
    {
        public SpawnPoint GetRespawnPoint(Vector3 position)
        {
            SpawnPoint best = null;
            float bestDistance = float.MaxValue;
            foreach (var spawnPoint in Values)
            {
                var distance = Vector3.Distance(position, spawnPoint.transform.position);
                if (distance < bestDistance)
                {
                    best = spawnPoint;
                    bestDistance = distance;
                }
            }
            return best;
        }
    }

}