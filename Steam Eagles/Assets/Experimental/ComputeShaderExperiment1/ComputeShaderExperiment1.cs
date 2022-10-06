using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderExperiment1 : MonoBehaviour
 {
     public ComputeShader computeShader;
     public RenderTexture renderTexture;
     public Material visualMaterial;
     private void Start()
     {
         int res = 256;
         
         renderTexture = new RenderTexture(res, res, 24);
         renderTexture.enableRandomWrite = true;
         renderTexture.Create();
         visualMaterial.SetTexture("_RenderTexture", renderTexture);
         computeShader.SetTexture(0, "Result", renderTexture);
         computeShader.SetFloat("Resolution", res);
 
         int threadGroupsX = renderTexture.width / 8;
         int threadGroupsY = renderTexture.height / 8;
         const int threadGroupsZ = 1;
         computeShader.Dispatch(0, threadGroupsX, threadGroupsY, threadGroupsZ);
     }
 
     private void OnRenderImage(RenderTexture src, RenderTexture dest)
     {
         if (renderTexture == null)
         {
             renderTexture = new RenderTexture(256, 256, 24);
             renderTexture.enableRandomWrite = true;
             renderTexture.Create();
         }
         
         Graphics.Blit(renderTexture, dest);
     }
 }
