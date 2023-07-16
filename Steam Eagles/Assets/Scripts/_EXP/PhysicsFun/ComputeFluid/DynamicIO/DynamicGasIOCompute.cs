using System;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public static class DynamicGasIOCompute
    {
        
        private static ComputeShader _computeShader;
        public static ComputeShader ComputeShader => _computeShader ? _computeShader : _computeShader = Resources.Load<ComputeShader>("DynamicGasIO");

        static int _dynamicGasIOKernel;
        static int _deleteGasInBoundaryKernel;
        private static readonly int _dynamicForceInputKernel;

        static DynamicGasIOCompute()
        {
            _dynamicGasIOKernel = ComputeShader.FindKernel("DynamicGasIO");
            _deleteGasInBoundaryKernel = ComputeShader.FindKernel("DeleteGasOnBoundaries");
            _dynamicForceInputKernel = ComputeShader.FindKernel("DynamicExternalForcesInput");
        }

        

        public static void ExecuteDynamicIO(RenderTexture gasState, RenderTexture velocity, ref DynamicIOData[] dynamicIO, 
            float maxPressure = 1, 
            float inputRandomMin = 0.9f, 
            float inputRandomMax = 1.1f)
        {
            if(dynamicIO ==null || dynamicIO.Length == 0) return;
            
            ComputeShader.SetTexture(_dynamicGasIOKernel, "gas", gasState);
            ComputeShader.SetTexture(_dynamicGasIOKernel, "gasVelocity",velocity);
            
            var buffer = new ComputeBuffer(dynamicIO.Length, DynamicIOData.Stride());
            buffer.SetCounterValue(0);
            buffer.SetData(dynamicIO);
            ComputeShader.SetBuffer(_dynamicGasIOKernel, "ioBuffer", buffer);
            ComputeShader.SetFloat("maxPressure", maxPressure);
            ComputeShader.SetInt("ioCount", dynamicIO.Length);
            int xThreads = dynamicIO.Length / 8;
            if(xThreads == 0) xThreads = 1;
            int yThreads = 1;
            ComputeShader.Dispatch(_dynamicGasIOKernel, xThreads, yThreads, 1);
            Array data = new DynamicIOData[dynamicIO.Length];
            buffer.GetData(data);
            buffer.Release();
            for (int i = 0; i < dynamicIO.Length; i++)
            {
                var io = dynamicIO[i];
                io.deltaIn = ((DynamicIOData[]) data)[i].deltaIn;
                io.deltaOut = ((DynamicIOData[]) data)[i].deltaOut;
                dynamicIO[i] = io;
            }
        }


        public static void ExecuteDeleteGasInBoundary(RenderTexture gasState, RenderTexture boundary)
        {
            Vector2Int boundaryScale = new Vector2Int(gasState.width/ boundary.width, gasState.height/boundary.height);
            Debug.Assert(boundaryScale.x == boundaryScale.y);

            const string BOUNDARY_SCALE_NAME = "boundaryScale";
            const string BOUNDARY_TEX_NAME = "boundary";
            const string GAS_TEX_NAME = "gasState";
            
            ComputeShader.SetInts(BOUNDARY_SCALE_NAME, boundaryScale.x, boundaryScale.y);
            ComputeShader.SetTexture(_deleteGasInBoundaryKernel, BOUNDARY_TEX_NAME, boundary);
            ComputeShader.SetTexture(_deleteGasInBoundaryKernel, GAS_TEX_NAME, gasState);
            int xThreads = boundary.width / 8;
            if(xThreads == 0) xThreads = 1;
            int yThreads = boundary.height / 8;
            if(yThreads == 0) yThreads = 1;
            ComputeShader.Dispatch(_deleteGasInBoundaryKernel, xThreads, yThreads, 1);
        }


        public static void ExecuteDynamicForce(RenderTexture velocityState, DynamicForceInput[] dynamicForceInputs)
        {
            ComputeShader.SetTexture(_dynamicForceInputKernel, "velocityTexture", velocityState);
            var buffer = new ComputeBuffer(dynamicForceInputs.Length, DynamicForceInput.Stride());
            buffer.SetData(dynamicForceInputs);
            int xThreads = dynamicForceInputs.Length / 8;
            if(xThreads == 0) xThreads = 1;
            int yThreads = 1;
            ComputeShader.SetBuffer(_dynamicForceInputKernel, "forceInputBuffer", buffer);
            ComputeShader.Dispatch(_dynamicForceInputKernel, xThreads, yThreads, 1);
            buffer.Release();
        }
    }

    public struct DynamicForceInput
    {
        public Vector2Int texel;
        public Vector2Int area;
        public Vector2 velocity;
        
        public static int Stride() => sizeof(int) * 4 + sizeof(float) * 2;
    }
}