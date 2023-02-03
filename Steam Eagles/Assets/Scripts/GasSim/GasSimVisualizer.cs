using System;
using System.Collections.Generic;
using UniRx;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Random = UnityEngine.Random;

namespace GasSim
{
    [RequireComponent(typeof(ParticleSystem), typeof(Grid))]
    public class GasSimVisualizer : MonoBehaviour
    {
        public uint seed = 100;
        public float sizeMultiplier = 1.5f;
        public bool useJobSystem = true;
        
        ParticleSystem ps;
        ParticleSystem PS => ps ? ps : ps = GetComponent<ParticleSystem>();
        
        private Grid _grid;
        private Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();

        private double baseTime;

        
        private void Awake()
        {
            baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
            // _rngs = new NativeArray<Unity.Mathematics.Random>(Unity.Jobs.LowLevel.Unsafe.JobsUtility.MaxJobThreadCount, Allocator.Persistent);
            // for (int i = 0; i < _rngs.Length; i++)
            // {
            //     _rngs[i] = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
            // }
        }

        private void OnDestroy()
        {
           // _rngs.Dispose();
        }

        ParticleSystem.Particle GetParticleForPressureCell(Vector2Int cell, int pressure, float updateRate)
        {
            float pressureOpacity = Mathf.InverseLerp(0, 16, pressure);
            Vector3Int cellPos = new Vector3Int(cell.x, cell.y, 0);
            Color particleColor = Color.white;
            float lifetime = updateRate + 0.1f;
            particleColor.a = pressureOpacity;
            uint seed = (uint)Random.Range(0, 10000);
            return new ParticleSystem.Particle
            {
                position = Grid.GetCellCenterWorld(cellPos),
                startColor = particleColor,
                startSize = Grid.cellSize.x * sizeMultiplier,
                startLifetime = lifetime,
                remainingLifetime = lifetime,
                randomSeed = seed,
                
            };
        }

        public void UpdateParticlesFromGridData(Dictionary<Vector2Int, int> cells, float updateRate)
        {
            if (!useJobSystem)
            {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cells.Count];
                int i = 0;
                foreach (var kvp in cells)
                {
                    var coord = kvp.Key;
                    var pressure = kvp.Value;
                    particles[i] = GetParticleForPressureCell(coord, pressure, updateRate);
                    i++;
                }
                PS.Emit(cells.Count);
                PS.SetParticles(particles, particles.Length);
            }
            else
            {
                var particleCount = cells.Count;
                
                var particleData = new NativeArray<ParticleSystem.Particle>(particleCount, Allocator.TempJob);
                var pressureData = new NativeArray<int>(particleCount, Allocator.TempJob);
                var positionData = new NativeArray<Vector3>(particleCount, Allocator.TempJob);
                var randomData = new NativeArray<uint>(particleCount, Allocator.TempJob);
     
                int i = 0;
                foreach (var cell in cells)
                {
                    var coord = new Vector3Int(cell.Key.x, cell.Key.y, 0);
                    pressureData[i] = cell.Value;
                    positionData[i] = Grid.GetCellCenterWorld(coord);
                    i++;
                }

                var calculateRandomSeedJobNonParallel = new CalculateRandomSeedJobNonParallel()
                {
                    randomData = randomData,
                    randomSeed = (int)(Time.realtimeSinceStartup * 100 + baseTime)
                };
                
                //var calculateRandomSeedJob = new CalculateRandomSeedJob() {
                //    randomData = randomData,
                //    time = (int)(Time.realtimeSinceStartup* 100 +baseTime)
                //    //rngs = _rngs
                //};
                var particleUpdateJob = new ParticleUpdateJob() {
                    cellSize = Grid.cellSize,
                    sizeMultiplier = sizeMultiplier,
                    particleCount = particleCount,
                    updateRate = updateRate,
                    particles = particleData,
                    pressureData = pressureData,
                    positionData = positionData,
                    randomData = randomData
                };
                var calculateRandomSeedJobHandle = calculateRandomSeedJobNonParallel.Schedule();
               // var calculateRandomSeedJobHandle = calculateRandomSeedJob.Schedule(particleCount, 64);

                particleUpdateJob.Schedule(PS, calculateRandomSeedJobHandle).Complete();
                PS.Emit(cells.Count);
                PS.SetParticles(particleData, particleCount);
                
                particleData.Dispose();
                pressureData.Dispose();
                positionData.Dispose();
                randomData.Dispose();
                
                
            }
        }

        struct CalculateRandomSeedJobNonParallel : IJob
        {
            [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex] public int threadId;
            [WriteOnly] public NativeArray<uint> randomData;
            public int randomSeed;
            public void Execute()
            {
                var random = new Unity.Mathematics.Random((uint)((threadId+1) * randomSeed));
                for (int i = 0; i < randomData.Length; i++)
                {
                    randomData[i] = random.NextUInt();
                }
            }
        }
        

        struct CalculateRandomSeedJob : IJobParallelFor
        {
            ///[Unity.Collections.LowLevel.Unsafe.NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> rngs;
            [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex] private int threadId;
            [WriteOnly] public NativeArray<uint> randomData;
            public int time;
            public void Execute(int index)
            {
               // randomData[index] = rngs[threadId].NextUInt();   
               var random = new Unity.Mathematics.Random((uint)((threadId+1) * time + index));
            }
        }

        struct ParticleUpdateJob : IJobParticleSystem
        {
            public float updateRate;
            public Vector2 cellSize;
            public int particleCount;
            public float sizeMultiplier;
            
            public NativeArray<uint> randomData;
            public NativeArray<int> pressureData;
            public NativeArray<ParticleSystem.Particle> particles;
            public NativeArray<Vector3> positionData;


            public void Execute(ParticleSystemJobData jobData)
            {
                for (int i = 0; i < particleCount; i++)
                {
                    var particle = particles[i];
                    var pressure = pressureData[i];
                    var position = positionData[i];
                    var pressureOpacity = Unity.Mathematics.math.unlerp(0, 16, pressure);
                    
                    var color = Color.white;
                    color.a = pressureOpacity;
                    
                    particle.startColor = color;
                    particle.startSize = cellSize.x * sizeMultiplier;
                    particle.startLifetime = updateRate + 0.1f;
                    particle.remainingLifetime = updateRate + 0.1f;
                    particle.position = position;
                    particle.randomSeed = randomData[i];
                    
                    particles[i] = particle;
                }
            }
        }
    }
}