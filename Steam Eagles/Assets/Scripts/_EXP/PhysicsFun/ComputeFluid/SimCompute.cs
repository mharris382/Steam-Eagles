using UnityEngine;

public static class SimCompute
{
    static ComputeShader _simCompute;
    public static ComputeShader SimComputeShader => _simCompute ? _simCompute : _simCompute = Resources.Load<ComputeShader>("Computes/SimCompute");
    
    
    
    
    public static void AssignIO(RenderTexture result, RenderTexture sinkTexture, RenderTexture sourceTexture, int sourceSize=1, int sinkSize=1, float srcMultiplier = 1, float sinkMultiplier = 1)
    {
        var kernel = SimComputeShader.FindKernel("SimIO");
        SimComputeShader.SetTexture(kernel, "Result", result);
        SimComputeShader.SetTexture(kernel, "sinks", sinkTexture);
        SimComputeShader.SetTexture(kernel, "sources", sourceTexture);
        SimComputeShader.SetFloat("sinkMultiplier", sinkMultiplier);
        SimComputeShader.SetFloat("sourceMultiplier", srcMultiplier);
        SimComputeShader.SetInt("sourceTexSize", sourceSize);
        SimComputeShader.SetInt("sinkTexSize", sinkSize);
    }

    public static void DispatchIO(RenderTexture result)
    {
        var kernel = SimComputeShader.FindKernel("SimIO");
        var threadsX = result.width / 8;
        var threadsY = result.height / 8;
        Debug.Assert(threadsX > 0);
        Debug.Assert(threadsY > 0);
        SimComputeShader.Dispatch(kernel, threadsX, threadsY, 1);
    }
}