using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid.Computes
{
    public static class ComputeShaders
    {
        static ComputeShader _boundaryHandlingCompute;
        static ComputeShader _navierStokesCompute;
        static ComputeShader _solversCompute;
        static ComputeShader _bufferUtilityCompute;
        static ComputeShader _userInputCompute;

        public static ComputeShader BoundaryHandlingCompute => _boundaryHandlingCompute;
        public static ComputeShader NavierStokesCompute => _navierStokesCompute;
        public static ComputeShader SolversCompute => _solversCompute;
        public static ComputeShader BufferUtilityCompute => _bufferUtilityCompute;
        public static ComputeShader UserInputCompute => _userInputCompute;
        
        static ComputeShaders()
        {
            _boundaryHandlingCompute = Resources.Load<ComputeShader>("Boundary_Handling");
            _navierStokesCompute = Resources.Load<ComputeShader>("Navier_Stokes");
            _solversCompute = Resources.Load<ComputeShader>("Solvers");
            _bufferUtilityCompute = Resources.Load<ComputeShader>("StructuredBufferUtility");
            _userInputCompute = Resources.Load<ComputeShader>("UserInput");
        }
    }
}