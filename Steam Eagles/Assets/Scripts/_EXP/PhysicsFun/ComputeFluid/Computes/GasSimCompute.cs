using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid.Computes
{
    public static class GasSimCompute
    {
        private static ComputeShader _gasSimCompute;
        private static int _updateVelocityKernel;
        private static int _updateGasStateKernel;
        private static int _mergeSourceSinkKernel;
        private static readonly int _velocityFromHolesKernel;
        private static readonly int _invertWallsKernel;
        private static ComputeShader Compute => _gasSimCompute ??= Resources.Load<ComputeShader>("GasSimCompute");
        static GasSimCompute()
        {
            _gasSimCompute = Resources.Load<ComputeShader>("GasSimCompute");
            _updateVelocityKernel = Compute.FindKernel("UpdateVelocity");
            _updateGasStateKernel = Compute.FindKernel("UpdateGasState");
            _mergeSourceSinkKernel = Compute.FindKernel("MergeSourceSink");
            _velocityFromHolesKernel = Compute.FindKernel("VelocityFromHoles");
            _invertWallsKernel = Compute.FindKernel("InvertWalls");
        }



        public static void UpdateVelocity(
            RenderTexture gasState,
            RenderTexture velocity,
            RenderTexture boundary,
            RenderTexture sourceSink, 
            float deltaTime)
        {
            const string VELOCITY_NAME = "velocityState";
            const string GAS_NAME = "gasStateTexture";
            const string BOUNDARY_NAME = "boundaryTexture";
            const string SOURCE_SINK_NAME = "sourceSinkTexture";
            
            const string IO_TEX_SIZE = "ioSize";
            const string BOUNDARY_TEX_SIZE = "boundarySize";
            const string DELTA_TIME_NAME = "deltaTime";
            const string GAS_TEX_SIZE = "gasTextureSize";
            
            Compute.SetTexture(_updateVelocityKernel, GAS_NAME, gasState);
            Compute.SetTexture(_updateVelocityKernel, VELOCITY_NAME, velocity);
            Compute.SetTexture(_updateVelocityKernel, BOUNDARY_NAME, boundary);
            Compute.SetTexture(_updateVelocityKernel, SOURCE_SINK_NAME, sourceSink);
            
            Compute.SetInts(GAS_TEX_SIZE, gasState.width, gasState.height);
            Compute.SetFloat(DELTA_TIME_NAME, deltaTime);

            int ioTextureSize = gasState.width / sourceSink.width;
            int boundaryTextureSize = gasState.width / boundary.width;
            
            Compute.SetInt(IO_TEX_SIZE, ioTextureSize);
            Compute.SetInt(BOUNDARY_TEX_SIZE, boundaryTextureSize);
            
            int xThreads = gasState.width / 16;
            if(xThreads == 0) xThreads = 1;
            int yThreads = gasState.height / 16;
            if(yThreads == 0) yThreads = 1;
            
            Compute.Dispatch(_updateVelocityKernel, xThreads, yThreads, 1);
        }
        
        
        public static void UpdateGasState(
            RenderTexture gasState,
            RenderTexture velocity,
            RenderTexture boundary,
            RenderTexture sourceSink, float deltaTime)
        {
            const string GAS_NAME = "gasState";
            const string VELOCITY_NAME = "velocityStateTexture";
            const string BOUNDARY_NAME = "boundaryTexture";
            const string SOURCE_SINK_NAME = "sourceSinkTexture";
            
            const string IO_TEX_SIZE = "ioSize";
            const string BOUNDARY_TEX_SIZE = "boundarySize";
            const string DELTA_TIME_NAME = "deltaTime";
            const string GAS_TEX_SIZE = "gasTextureSize";
            
            Compute.SetTexture(_updateGasStateKernel, GAS_NAME, gasState);
            Compute.SetTexture(_updateGasStateKernel, VELOCITY_NAME, velocity);
            Compute.SetTexture(_updateGasStateKernel, BOUNDARY_NAME, boundary);
            Compute.SetTexture(_updateGasStateKernel, SOURCE_SINK_NAME, sourceSink);
            Compute.SetInts(GAS_TEX_SIZE, gasState.width, gasState.height);
            Compute.SetFloat(DELTA_TIME_NAME, deltaTime);
            
            int ioTextureSize = gasState.width / sourceSink.width;
            int boundaryTextureSize = gasState.width / boundary.width;
            
            Compute.SetInt(IO_TEX_SIZE, ioTextureSize);
            Compute.SetInt(BOUNDARY_TEX_SIZE, boundaryTextureSize);

            
            int xThreads = gasState.width / 16;
            if(xThreads == 0) xThreads = 1;
            int yThreads = gasState.height / 16;
            if(yThreads == 0) yThreads = 1;
            Compute.Dispatch(_updateGasStateKernel, xThreads, yThreads, 1);
        }




        public static void MergeSourceSinks(
            RenderTexture sourceTexture,
            RenderTexture sinkTexture,
            RenderTexture mergeTexture)
        {
            const string SOURCE_NAME = "sourceTexture";
            const string SINK_NAME = "sinkTexture";
            const string IO_Name = "ioTexture";
            const string SRC_SIZE = "sourceTexSize";
            const string SINK_SIZE = "sinkTexSize";
            Compute.SetTexture(_mergeSourceSinkKernel, SOURCE_NAME, sourceTexture);
            Compute.SetTexture(_mergeSourceSinkKernel, SINK_NAME, sinkTexture);
            Compute.SetTexture(_mergeSourceSinkKernel, IO_Name, mergeTexture);
            
            int sinkTextureSize =  Mathf.Max(1, mergeTexture.width / sinkTexture.width);
            int srcTextureSize = Mathf.Max(1, mergeTexture.width / sourceTexture.width); 
            Compute.SetInt(SRC_SIZE, srcTextureSize);
            Compute.SetInt(SINK_SIZE, sinkTextureSize);
            
            int xThreads = mergeTexture.width / 16;
            if(xThreads == 0) xThreads = 1;
            int yThreads = mergeTexture.height / 16;
            if(yThreads == 0) yThreads = 1;
            
            Compute.Dispatch(_mergeSourceSinkKernel, xThreads, yThreads, 1);
        }


        public static void UpdateVelocityFromHoles(RenderTexture velocity, RenderTexture holes)
        {
            const string VELOCITY_NAME = "velocityState";
            const string HOLDES_NAME = "holesTexture";
            const string HOLE_TEX_SIZE = "holeTexSize";
            
            
            Compute.SetTexture(_velocityFromHolesKernel, VELOCITY_NAME, velocity);
            Compute.SetTexture(_velocityFromHolesKernel, HOLDES_NAME, holes);
            
            int holeTextureSize = velocity.width / holes.width;
            Compute.SetInt(HOLE_TEX_SIZE, holeTextureSize);
            
            int xThreads = velocity.width / 16;
            if(xThreads == 0) xThreads = 1;
            int yThreads = velocity.height / 16;
            if(yThreads == 0) yThreads = 1;
            Compute.Dispatch(_velocityFromHolesKernel, xThreads, yThreads, 1);
        }

        public static void InvertWalls(RenderTexture walls)
        {
            const string WALL_NAME = "wallsTexture";
            Compute.SetTexture(_invertWallsKernel, WALL_NAME, walls);
            int xThreads = walls.width / 16;
            if(xThreads == 0) xThreads = 1;
            int yThreads = walls.height / 16;
            if(yThreads == 0) yThreads = 1;
            Compute.Dispatch(_invertWallsKernel, xThreads, yThreads, 1);
        }
    }
}