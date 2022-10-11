using UnityEngine;

namespace World.GasSim
{
    public class SimulationController : MonoBehaviour
    {
        public ComputeShader simulationComputeShader;
        public SharedRenderTexture solidRenderTexture;
        public SharedRenderTexture velocityFieldTexture;
        
    }

   
}