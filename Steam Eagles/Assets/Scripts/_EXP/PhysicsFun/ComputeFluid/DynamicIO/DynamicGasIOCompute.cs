using System;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public static class DynamicGasIOCompute
    {
        
        private static ComputeShader _computeShader;
        public static ComputeShader ComputeShader => _computeShader ? _computeShader : _computeShader = Resources.Load<ComputeShader>("DynamicGasIO");

        static int _dynamicGasIOKernel;
        static DynamicGasIOCompute()
        {
            _dynamicGasIOKernel = ComputeShader.FindKernel("DynamicGasIO");
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
    }
}