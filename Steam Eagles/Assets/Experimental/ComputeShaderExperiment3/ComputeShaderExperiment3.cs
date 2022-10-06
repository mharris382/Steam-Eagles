using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ComputeShaderExperiment3 : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader simulationShader;
    public ComputeShader computeVelocityShader;
    
    public Texture initialState;
    [FormerlySerializedAs("targetTexture")] public RenderTexture renderTexture;
    public Material targetMaterial;


    public RenderTexture vectorFieldTexture;
    
    private static readonly int RenderTexture1 = Shader.PropertyToID("_RenderTexture");
    public Vector2 airVelocity;
    public float updateTime = 0.2f;
    public bool simRunning = true;
    private void Start()
    {
        var resolution = initialState.width;
       
        CreateRenderTextures();
        
        computeShader.SetFloat("resolution", resolution);
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetTexture(0, "Initial", initialState);

        int threadGroupsX = renderTexture.width / 8;
        int threadGroupsY = renderTexture.height / 8;
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    void CreateRenderTextures()
    {
        var resolution = initialState.width;
        renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.RInt);
        vectorFieldTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.RGFloat);
        vectorFieldTexture.enableRandomWrite = true;
        renderTexture.enableRandomWrite = true;
        vectorFieldTexture.Create();
        renderTexture.Create();
        targetMaterial.SetTexture(RenderTexture1, renderTexture);
    }
    
    IEnumerator Sim()
    {
        while (simRunning)
        {
            UpdateSimulation();
            yield return new WaitForSeconds(updateTime);
        }
    }

    void UpdateSimulation()
    {
        if (renderTexture == null)
        {
            return;
        }
        var resolution = renderTexture.width;
        var solidColor = new Color(0, 0, 0, 1);
        var gasColor = new Color(1, 1, 1, 1);
        simulationShader.SetVector("solid", solidColor);
        simulationShader.SetVector("gas", gasColor);
        simulationShader.SetFloat("resolution", resolution);
        simulationShader.SetTexture(0, "Result", renderTexture);
        int threadGroupsX = renderTexture.width / 8;
        int threadGroupsY = renderTexture.height / 8;
        simulationShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Simulation Step"))
        {
            UpdateSimulation();
        }

        if (GUI.Button(new Rect(0, 50, 100, 50), "Run Simulation"))
        {
            StartCoroutine(nameof(Sim));
        }
        if (GUI.Button(new Rect(0, 100, 100, 50), "Stop Simulation"))
        {
            StopAllCoroutines();
        }
    }
}
