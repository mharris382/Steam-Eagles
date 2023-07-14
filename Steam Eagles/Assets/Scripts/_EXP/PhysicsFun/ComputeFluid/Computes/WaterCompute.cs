using CoreLib.Extensions;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid.Computes
{
    public static class WaterCompute
    {
        private static ComputeShader _sahder;
        public static ComputeShader Shader => _sahder ? _sahder : _sahder = Resources.Load<ComputeShader>("ComputeWater");

        const string WATER_TEX = "waterTexture";
        const string RW_WATER_TEX = "waterState";
        
        const string VELOCITY_TEX = "waterTexture";
        const string RW_VELOCITY_TEX = "velocityState";
        
        const string BOUNDARY_TEX = "boundaryTexture";
        const string STATIC_IO_TEX = "sourceTexture";
        
        const string DELTA_TIME = "deltaTime";
        
        private static  int _updateVelocityKernel;
        private static  int _moveWaterKernel;
        private static  int _sampleWaterKernel;

        
        public struct WaterIOData
        {
            public Vector2Int waterTexCoord;
            public float waterDelta;
            public Vector2 velocity;
            public static int Stride() => sizeof(int) * 2 + sizeof(float) + sizeof(float) * 2;
        }

        public struct WaterSampleData
        {
            public Vector2Int waterTexCoord;
            public float water;
            public Vector2 velocity;
            
            public static int Stride() => sizeof(int) * 2 + sizeof(float) + sizeof(float) * 2;
        }
        static WaterCompute()
        {
            _updateVelocityKernel = Shader.FindKernel("UpdateVelocity");
            _moveWaterKernel = Shader.FindKernel("MoveWater");
            _sampleWaterKernel = Shader.FindKernel("SampleWater");
        }

        static void SetWaterTexture(int kernel, RenderTexture waterTexture, bool isReadOnly)
        {
            Shader.SetTexture(kernel, isReadOnly ? WATER_TEX : RW_WATER_TEX, waterTexture);
        }
        static void SetVelocityTexture(int kernel, RenderTexture velocityTexture, bool isReadOnly)
        {
            Shader.SetTexture(kernel, isReadOnly ? WATER_TEX : RW_WATER_TEX, velocityTexture);
        }
        static void SetBoundaryTexture(int kernel,RenderTexture renderTexture)
        {
            Shader.SetTexture(kernel, BOUNDARY_TEX, renderTexture);
        }
        
        
        public static void UpdateWaterVelocity(RenderTexture water, RenderTexture waterVelocity, RenderTexture boundary, RenderTexture staticIO, float deltaTime)
        {
            if (!water.SizeMatches(waterVelocity, boundary, staticIO))
            {
                Debug.LogError("WaterCompute.UpdateWaterState: RenderTextures do not match");
                return;
            }
            SetWaterTexture(_updateVelocityKernel, water, false);
            SetVelocityTexture(_updateVelocityKernel, waterVelocity, true);
            SetBoundaryTexture(_updateVelocityKernel, boundary);
            Shader.SetFloat(DELTA_TIME, deltaTime);
            Shader.Dispatch(_updateVelocityKernel, water.width / 8, water.height / 8, 1);
        }
        public static void UpdateWaterState(RenderTexture water, RenderTexture waterVelocity, RenderTexture boundary, RenderTexture staticIO, float deltaTime)
        {
            if (!water.SizeMatches(waterVelocity, boundary, staticIO))
            {
                Debug.LogError("WaterCompute.UpdateWaterState: RenderTextures do not match");
                return;
            }
            SetWaterTexture(_updateVelocityKernel, water, true);
            SetVelocityTexture(_updateVelocityKernel, waterVelocity, false);
            SetBoundaryTexture(_updateVelocityKernel, boundary);
            Shader.SetFloat(DELTA_TIME, deltaTime);
            Shader.Dispatch(_updateVelocityKernel, water.width / 8, water.height / 8, 1);
        }
    }
}