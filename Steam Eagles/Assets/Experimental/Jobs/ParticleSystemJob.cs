using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Particle = UnityEngine.ParticleSystem.Particle;

namespace Experimental.Jobs
{
    public class ParticleSystemJob : MonoBehaviour
    {
        public bool useJob = true;
        
        public AlphaToPressureMap alphaToPressureMap = new AlphaToPressureMap() {
            pressureRange = new Vector2(0, 15),
            alphaRange = new Vector2(0.1f, .8f)
        };
       
        private ParticleSystem _ps;
        private UpdateParticlesJob _job;
        
        private Particle[] _mainThreadParticles;
        private NativeArray<Particle> _mainThreadParticlesNative;
        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            _mainThreadParticlesNative = new NativeArray<Particle>(_ps.main.maxParticles, Allocator.Persistent);
        }

        #region [Main Thread Update]

        private void Update()
        {
            if (!useJob)
            {
                if (_mainThreadParticles == null)
                    _mainThreadParticles = new Particle[_ps.main.maxParticles];//uses main count so array will always be big enough
                var count = _ps.GetParticles(_mainThreadParticles);
                for (int i = 0; i < count; i++) {
                    DoCalculationsOnMainThread(i);
                }
                _ps.SetParticles(_mainThreadParticles, count);
            }
        }

        private void DoCalculationsOnMainThread(int i)
        {
            Vector3 position = _mainThreadParticles[i].position;
            Vector3 delta = position - _job.effectPosition;
            if (delta.sqrMagnitude < _job.effectRangeSqr)
            {
                _mainThreadParticles[i].velocity += CalculateVelocity(ref _job, delta);
                _mainThreadParticles[i].startColor = CalculateColor(ref _job, delta, _mainThreadParticles[i].startColor,
                    _mainThreadParticles[i].randomSeed);
            }
        }
        

        #endregion
        
        #region [Static Helpers]

        private static Vector3 CalculateVelocity(ref UpdateParticlesJob job, Vector3 delta)
        {
            float attraction = job.effectStrength / job.effectRangeSqr;
            return delta.normalized * attraction;
        }
        
        private static Color32 CalculateColor(ref UpdateParticlesJob job, Vector3 delta, Color32 srcColor, UInt32 seed)
        {
            var targetColor = new Color32((byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), srcColor.a);
            float lerpAmount = delta.magnitude * job.inverseEffectRange;
            lerpAmount = lerpAmount * 0.25f + 0.75f;
            return Color32.Lerp(targetColor, srcColor, lerpAmount);
        }

        #endregion

        #region [Job]

        private void OnParticleUpdateJobScheduled()
        {
            if (useJob)
                _job.Schedule(_ps, 2048);
        }

        private UpdateParticlesJob CreateNewUpdateParticlesJob()
        {
            var job = new UpdateParticlesJob()
            {

            };

            return job;
        }
        
        
        struct UpdateParticlesJob : IJobParticleSystemParallelFor
        {
            [ReadOnly]
            public AlphaToPressureMap alphaToPressureMap;
            
            [ReadOnly]
            public Vector3 effectPosition;

            [ReadOnly]
            public float effectRangeSqr;

            [ReadOnly]
            public float effectStrength;

            [ReadOnly]
            public float inverseEffectRange;

            public void Execute(ParticleSystemJobData particles, int i)
            {
                var positionsX = particles.positions.x;
                var positionsY = particles.positions.y;
                var positionsZ = particles.positions.z;

                var velocitiesX = particles.velocities.x;
                var velocitiesY = particles.velocities.y;
                var velocitiesZ = particles.velocities.z;

                var colors = particles.startColors;

                var randomSeeds = particles.randomSeeds;

                Vector3 position = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);
                Vector3 delta = (position - effectPosition);
                if (delta.sqrMagnitude < effectRangeSqr)
                {
                    Vector3 velocity = CalculateVelocity(ref this, delta);

                    velocitiesX[i] += velocity.x;
                    velocitiesY[i] += velocity.y;
                    velocitiesZ[i] += velocity.z;

                    colors[i] = CalculateColor(ref this, delta, colors[i], randomSeeds[i]);
                }
            }
        }

        #endregion
    }
    
    [Serializable]
    public struct AlphaToPressureMap
    {
        public Vector2 pressureRange;
        public Vector2 alphaRange;

        public int MapAlphaToPressure(float alpha) => Mathf.RoundToInt(MapAlphaToPressureNormalized(alpha) * 15);
        public float MapAlphaToPressureNormalized(float alpha) => Remap(alpha, alphaRange.x, alphaRange.y, pressureRange.x, pressureRange.y);

        public float MapPressureToAlpha(float pressure) => Remap(pressure, pressureRange.x, pressureRange.y, alphaRange.x, alphaRange.y);

        private static float Remap(float x, float x1, float x2, float y1, float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1;

            return m * x + c;
        }
    }
}