using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

public class SolidRenderTexture : MonoBehaviour
{
    public SharedTilemap solidTilemap;
    public SharedRenderTexture liveRenderTexture;
    
    public RenderTexture currentRenderTexture;
    public Camera camera;
    public LayerMask layerMask;
    private void Start()
    {
        if (solidTilemap.Value != null)
        {
            OnTilemapAssigned(solidTilemap.Value);
        }
        solidTilemap.onValueChanged.AddListener(OnTilemapAssigned);
    }

    private void OnDestroy()
    {
        solidTilemap.onValueChanged.RemoveListener(OnTilemapAssigned);
    }

    void OnTilemapAssigned(Tilemap tilemap)
    {
        if (tilemap == null)
        {
            
        }
        else
        {
            var bounds = tilemap.cellBounds;
            int width = bounds.size.x;
            int height = bounds.size.y;
            var pos = new Vector3(bounds.center.x, bounds.center.y, -10);
            currentRenderTexture = new RenderTexture(width, height, 24);
            currentRenderTexture.enableRandomWrite = true;
            currentRenderTexture.Create();
            if (camera == null)
            {
                var go = new GameObject("Solid Tilemap Render Camera", typeof(Camera));
                camera = go.GetComponent<Camera>();
            }
            camera.targetTexture = currentRenderTexture;
            camera.transform.position = pos;
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Min(width, height) / 2f;
            camera.backgroundColor = Color.white;
            camera.cullingMask = layerMask;
            liveRenderTexture.Value = currentRenderTexture;
        }
    }

    void ClearRenderTexture()
    {
        liveRenderTexture.Value = null;
    }
}