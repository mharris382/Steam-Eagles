using System;
using UnityEngine;

namespace Tests.Gas_Simulation_Compute_Shaders_Test
{
    public class GasSimulationComputeShaderTest : MonoBehaviour
    {
        public RenderTexture rt;
        public ParticleSystem ps;
        
        public SimulationGPUResources gpuResources;
        
    }


    [Serializable]
    public class SimulationGPUResources
    {
        public ComputeShader pressureCompute;
        public ComputeShader blobSourceCompute;
        
        public ComputeShader bufferToTextureCompute;
        public ComputeShader textureToBufferCompute;
    }
}