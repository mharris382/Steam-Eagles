using System;
using System.Collections.Generic;

using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemSource : MonoBehaviour
    {
        private ParticleSystem _ps;
        private ParticleSystem.Particle[] _particles;

        private Queue<(Vector3Int cell, int amount, int checkCount)> _particleQueue =
            new Queue<(Vector3Int cell, int amount, int checkCount)>();
        
        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (_particles == null || _particles.Length < _ps.main.maxParticles)
            {
                _particles = new ParticleSystem.Particle[_ps.main.maxParticles];
            }

            int count = _ps.GetParticles(_particles);
            for (int i = 0; i < count; i++)
            {
                if(_particles[i].remainingLifetime < 0.1f)
                    _particleQueue.Enqueue((Vector3Int.FloorToInt(_particles[i].position), 1, 0));
            }
            _ps.SetParticles(_particles, count);
        }
    }
}