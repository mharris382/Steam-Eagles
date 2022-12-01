using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FluidCamera : MonoBehaviour
{
    public Vector2Int resolution = new Vector2Int(512, 512);


    [Required]
    public GameObject fluidRendererGO;

    private IFluidRenderer _fluidRenderer;
    
    private void Awake()
    {
        _fluidRenderer= fluidRendererGO.GetComponent<IFluidRenderer>();
        var _camera = GetComponent<Camera>(); 
        
        Debug.Assert(_fluidRenderer != null, this);

        var rt = new RenderTexture(resolution.x, resolution.y, 32);
        rt.enableRandomWrite = true;
        rt.Create();
        _camera.targetTexture = rt;
        _fluidRenderer.SetFluidTexture(rt);
    }
}

public interface IFluidRenderer
{
    void SetFluidTexture(RenderTexture renderTexture);
}