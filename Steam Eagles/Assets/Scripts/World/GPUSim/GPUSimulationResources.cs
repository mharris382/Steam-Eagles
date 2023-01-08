using UnityEngine;

namespace GasSim.GPUSim
{
    public class GPUSimulationResources
    {
        public int simulationDimensions;
        public ComputeBuffer buffer_1;
        public ComputeBuffer buffer_2;

        public GPUSimulationResources(int simulationDimensions)
        {
            this.simulationDimensions = simulationDimensions;
        }
    }
}