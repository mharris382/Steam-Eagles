using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;


namespace GasSim
{
    public class GasSimParticleSystemMk2 : GasSimParticleSystem
    {
    
        public bool useJobForUpdateParticles = true;
        public bool useJobForGridMovements = false;

        
        private UpdateParticleFromGrid _updateParticleFromGridJob = new UpdateParticleFromGrid();
        private JobHandle _updateParticleFromGridJobHandle;
        private NativeArray<GasCell> _nonEmptyCellsNative;

        protected override void Start()
        {
            
            StartCoroutine(RunSimulation());
            base.Start();
        }

        IEnumerator RunSimulation()
        {
            while (enabled)
            {
                // Schedule Early
                DoGridMovements();
                yield return new WaitForSeconds(updateRate/2f);
                
               
                if (useJobForUpdateParticles)
                {
                    yield return ScheduleCopyJob();
                }
                else
                {
                    base.UpdateParticlesFromGrid();
                    
                }
            }
        }

        private void CleanupJob()
        {
            
        }

        protected override void DoSimulationStep1()
        {
            //base.DoSimulationStep1();
        }

        protected override void DoSimulationStep2()
        {
            //base.DoSimulationStep2();
        }


        protected override void DoGridMovements()
        {
            if(!useJobForGridMovements)
                base.DoGridMovements();
        }

        protected override void UpdateParticlesFromGrid()
        {
            if(!useJobForUpdateParticles)
                base.UpdateParticlesFromGrid();
        }

        private void OnParticleUpdateJobScheduled()
        {
            if (!useJobForUpdateParticles)
                return;
            var nonEmptyCells = InternalPressureGrid.GetAllNonEmptyCells().Select(t=> new GasCell(t.coord, t.pressure)).ToArray();
            _nonEmptyCellsNative = new NativeArray<GasCell>(nonEmptyCells, Allocator.TempJob);
            _updateParticleFromGridJob.lifetime = 1f/updateRate;
            _updateParticleFromGridJob.alphaMax = this.pressureColor.PressureToColor(15).a;
            _updateParticleFromGridJob.nonEmptyCells = _nonEmptyCellsNative;
            this._updateParticleFromGridJobHandle = _updateParticleFromGridJob.Schedule(this.ParticleSystem, 2048);
            _updateParticleFromGridJobHandle.Complete();
            _updateParticleFromGridJobHandle = default;
            _nonEmptyCellsNative.Dispose();
        }


        IEnumerator ScheduleCopyJob()
        {
           
           //yield return new WaitForSeconds(updateRate / 2f);
           yield break;
        }

       
        struct UpdateParticleFromGrid : IJobParticleSystemParallelFor
        {
            public NativeArray<GasCell> nonEmptyCells;
            public float alphaMax;
            public float lifetime;
            public void Execute(ParticleSystemJobData particles, int i)
            {
                var positionsX = particles.positions.x;
                var positionsY = particles.positions.y;
                var positionsZ = particles.positions.z;

                var velocitiesX = particles.velocities.x;
                var velocitiesY = particles.velocities.y;
                var velocitiesZ = particles.velocities.z;
                var startLifetimes = particles.inverseStartLifetimes;
                var colors = particles.startColors;
                
                var randomSeeds = particles.randomSeeds;
                Vector3 position = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);
                Vector3 velocity = new Vector3(velocitiesX[i], velocitiesY[i], velocitiesZ[i]);
                startLifetimes[i] = this.lifetime;
                if(i >= nonEmptyCells.Length)
                    return;
                
                var gasCell = nonEmptyCells[i];
                
                velocity= ParticleGetVelocity(new Vector2Int(gasCell.x, gasCell.y), gasCell.pressure);
                velocitiesX[i] = velocity.x;
                velocitiesY[i] = velocity.y;
                
                
                
                var pressure = gasCell.pressure;
                var alpha = (pressure / 16f) * alphaMax;
                var color = colors[i];
                color.a =  (byte)Mathf.Round(Mathf.Clamp01(alpha) * (float)byte.MaxValue);
                colors[i] = color;
            }
        }
        
        struct GasCell
        {
            public int x;
            public int y;
            public byte pressure;
            

            public GasCell(Vector2Int coord, int pressure)
            {
                x = coord.x;
                y = coord.y;
                this.pressure = (byte)pressure;
            }
            public GasCell(int x, int y, byte pressure)
            {
                this.x = x;
                this.y = y;
                this.pressure = pressure;
            }
        }

    }
}