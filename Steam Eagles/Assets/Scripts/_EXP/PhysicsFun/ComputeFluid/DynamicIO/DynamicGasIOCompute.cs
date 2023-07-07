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
        static DynamicGasIOCompute()
        {
            _dynamicGasIOKernel = ComputeShader.FindKernel("DynamicGasIO");
            _deleteGasInBoundaryKernel = ComputeShader.FindKernel("DeleteGasOnBoundaries");
        }


        public static void ExecuteDynamicIO(RenderTexture gasState, ref DynamicIOData[] dynamicIO)
        {
            if(dynamicIO ==null || dynamicIO.Length == 0) return;
            ComputeShader.SetTexture(_dynamicGasIOKernel, "gas", gasState);
            var buffer = new ComputeBuffer(dynamicIO.Length, DynamicIOData.Stride());
            buffer.SetCounterValue(0);
            buffer.SetData(dynamicIO);
            ComputeShader.SetBuffer(_dynamicGasIOKernel, "ioBuffer", buffer);
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
    }
}