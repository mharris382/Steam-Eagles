using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using World;

public class VelocityFieldRenderTexture : MonoBehaviour
{
    private const int X_THREADS = 8;
    private const int Y_THREADS = 8;

    public Color airMask;
    [Min(1)]
    public int processedScale = 1;
   
    

    [Tooltip("processes the raw camera texture so that it is ready to be used by the fluid simulation")]
    public ComputeShader processSolidComputeShader;
    
    [Header("Input")] [FormerlySerializedAs("solidRenderTexture")]
    [Tooltip("Render texture directly from the environment mapping camera")]
    public SharedRenderTexture solidTextureRaw;
    
    [Header("Output Textures")]
    
    [Tooltip("texture reference which will be created and store the solid environment map")]
    public SharedRenderTexture solidTextureProcessed;
    
    public bool initializeGasTexture= true;
    public SharedRenderTexture gasTexture;
    
    public bool initializeVelocityTexture= true;
    [Tooltip("texture reference which will be created and store the solid environment map")]
    public SharedRenderTexture velocityFieldTexture;
    public SharedRenderTexture previousVelocityTexture;
    public int gasBlockPerSolidBlock = 4;
    

    private int GasPerSolid => gasBlockPerSolidBlock;

    private bool isNull = true;
    private void Awake()
    {
        solidTextureRaw.onValueChanged.AddListener(OnSolidRenderTextureChanged);
        if (solidTextureRaw.Value != null) 
            OnSolidRenderTextureChanged(solidTextureRaw.Value);
    }

    private IEnumerator Start()
    {
        while (!solidTextureRaw.HasValue)
        {
            Debug.Log("Waiting on Solid Render Texture Raw...");
            yield return null;
        }
       OnSolidRenderTextureChanged(solidTextureRaw.Value);
    }

    private void OnDestroy()
    {
        solidTextureRaw.onValueChanged.RemoveListener(OnSolidRenderTextureChanged);
    }

    void OnSolidRenderTextureChanged(RenderTexture raw)
    {
        if (raw == null)
        {
            if(!isNull)
                Debug.LogError($"Why did solid render texture: {solidTextureRaw} become RAW?", this);
            isNull = true;
            return;
        }
        int w = raw.width;
        int h = raw.height;
        CreateDynamicRenderTexture(w, h ,solidTextureProcessed);
        ProcessSolidTexture();
        if (initializeGasTexture)
        {
            int wGas = (w * gasBlockPerSolidBlock);
            int hGas = (h * gasBlockPerSolidBlock);
            CreateDynamicRenderTexture(wGas, hGas, gasTexture);
           
        }

        if (initializeVelocityTexture)
        {
            int wVel = (w * gasBlockPerSolidBlock);
            int hVel = (h * gasBlockPerSolidBlock);
            CreateDynamicRenderTexture(wVel, hVel, velocityFieldTexture);
            CreateDynamicRenderTexture(wVel, hVel, previousVelocityTexture);
        }
    }
    void ProcessSolidTexture()
    {
        
         processSolidComputeShader.SetInt("processedScale", processedScale);
         processSolidComputeShader.SetTexture(0 , "raw", solidTextureRaw.Value);
         processSolidComputeShader.SetTexture(0 , "solid", solidTextureProcessed.Value);
         processSolidComputeShader.SetVector("airMask", new Vector4(airMask.r, airMask.g, airMask.b, airMask.a));
         processSolidComputeShader.SetInts("size", solidTextureProcessed.Value.width, solidTextureProcessed.Value.height);
         int xThreadGroups = solidTextureProcessed.Value.width / X_THREADS;
         int yThreadGroups = solidTextureProcessed.Value.height/ Y_THREADS; 
         processSolidComputeShader.Dispatch(0, xThreadGroups, yThreadGroups, 1);
    }
    void CreateDynamicRenderTexture(int width, int height, SharedRenderTexture variable)
    {
        var fieldTexture = new RenderTexture(width, height, 1)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point,
            depth = 16
        };
        fieldTexture.Create();
        variable.Value = fieldTexture;
    }


    private void Update()
    {
        ProcessSolidTexture();
    }
    //
    //
    // void CreateSolidTexture(Texture raw)
    // {
    //     var shape = GetGasRect();
    //     var texture = new RenderTexture(shape.width, shape.height, 24);
    //     texture.enableRandomWrite = true;
    //     texture.Create();
    //     ProcessSolidTexture(raw, texture);
    //     solidTextureProcessed.Value = texture;
    // }
    //
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    //
    // RectInt GetGasRect()
    // {
    //     var bounds = solidRenderTexture.solidTilemap.Value.cellBounds;
    //     return new RectInt(bounds.xMin, bounds.xMin, bounds.size.x, bounds.size.y);
    // }
}