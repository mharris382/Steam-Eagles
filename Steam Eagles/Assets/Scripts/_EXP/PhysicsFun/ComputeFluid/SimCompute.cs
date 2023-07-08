using UnityEngine;

public static class SimCompute
{
    static ComputeShader _simCompute;
    public static ComputeShader SimComputeShader => _simCompute ? _simCompute : _simCompute = Resources.Load<ComputeShader>("Computes/SimCompute");


    static SimCompute()
    {
        
    }
    
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

    public static void AssignDiffuse(RenderTexture gasTexture, RenderTexture boundaryTexture, float laplacianCenter = -4.0f,    float laplacianNeighbor = 1.0f, float laplacianDiagnal = 0.5f)
    {
        var kernel = SimComputeShader.FindKernel("SimDiffuse");
        SimComputeShader.SetTexture(kernel, "gas", gasTexture);
        SimComputeShader.SetTexture(kernel, "boundaryTexture", boundaryTexture);
        SimComputeShader.SetInts("boundaryTextureScale", boundaryTexture.width/gasTexture.width, boundaryTexture.height/gasTexture.height);
        SimComputeShader.SetFloat("laplacianCenter", laplacianCenter);
        SimComputeShader.SetFloat("laplacianNeighbor", laplacianNeighbor);
        SimComputeShader.SetFloat("laplacianDiagnal", laplacianDiagnal);
        int threadsX = gasTexture.width / 8;
        int threadsY = gasTexture.height / 8;
        Debug.Assert(threadsX > 0);
        Debug.Assert(threadsY > 0);
        SimComputeShader.Dispatch(kernel, threadsX, threadsY, 1);
    }

    public static void DispatchDiffuse()
    {
        
    }

    public static void AssignGasSampler(RenderTexture gasTexture, ref GasSampleData[] sampleData)
    {
            var kernel = SimComputeShader.FindKernel("SampleSimPoints");
        SimComputeShader.SetTexture(kernel, "sampleTarget", gasTexture);
        var buffer = new ComputeBuffer(sampleData.Length, GasSampleData.Stride());
        buffer.SetData(sampleData);
        SimComputeShader.SetBuffer(kernel, "samplePoints", buffer);
        int threadsX = sampleData.Length / 8;
        int threadsY = 1;
        if(threadsX < 1) threadsX = 1;
        SimComputeShader.Dispatch(kernel, threadsX, threadsY, 1);
        buffer.GetData(sampleData);
        buffer.Release();
    }
}


public struct GasSampleData
{
    public Vector2Int texel;
    public float gasValue;
    public Matrix3x3 gradient;
    
    
    public static int Stride() => sizeof(int) * 2 + sizeof(float) + sizeof(float) * 9;
}


public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;

    public Matrix3x3(
        float m00, float m01, float m02,
        float m10, float m11, float m12,
        float m20, float m21, float m22)
    {
        this.m00 = m00; this.m01 = m01; this.m02 = m02;
        this.m10 = m10; this.m11 = m11; this.m12 = m12;
        this.m20 = m20; this.m21 = m21; this.m22 = m22;
    }
    
    
    public float Up => m01;
    public float Down => m21;
    public float Left => m10;
    public float Right => m12;
    
    public float Center => m11;
    
    public float DiagonalUpLeft => m00;
    public float DiagonalUpRight => m02;
    public float DiagonalDownLeft => m20;
    public float DiagonalDownRight => m22;



    public float Maximum()
    {
        return Mathf.Max(m00, m01, m02, m10, m11, m12, m20, m21, m22);
    }
    public float Average()
    {
        return  Sum() / 9.0f;
    }

    public float Sum()
    {
        return (m00 + m01 + m02 + m10 + m11 + m12 + m20 + m21 + m22);
    }
}