using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderExperiment3 : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture sourceTexture;
    public RenderTexture targetTexture;
    public Material targetMaterial;
    
    public int gridSize = 256;
    
    private void Start()
    {
        
    }
}
