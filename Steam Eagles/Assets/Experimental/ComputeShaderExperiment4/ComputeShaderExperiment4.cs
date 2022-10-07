using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct Cell
{
    private Vector2Int cell;
    private Vector2Int velocity;
    private int density;
    private int cellType;
}
public class ComputeShaderExperiment4 : MonoBehaviour
{
    public const int SOLID_ID = 3;
    public const int SOURCE_ID = 1;
    public const int AIR_ID = 0;
    
    public int gridSize = 128;

    public Sprite sprite;

    public ComputeShader computeShader;


    public RenderTexture resultTexture;
    public Vector3Int sourceCell = new Vector3Int(10, 10, 3);
    
    private Cell[] cells;
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        resultTexture = new RenderTexture(gridSize, gridSize, 24);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();

        cells = new Cell[gridSize * gridSize];
        renderers = new SpriteRenderer[gridSize * gridSize];
    }

    private void CreateCell(int x, int y)
    {
        GameObject r = new GameObject($"Cube {x * gridSize}, {y * gridSize}", typeof(SpriteRenderer));
        var sr = r.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        r.transform.position = new Vector3(x, y);
        renderers[x * gridSize + y] = sr;
        cells[x * gridSize + y] = new Cell()
        {
            
        };
    }

    private void ExecuteCompute()
    {
        computeShader.SetTexture(0, "Result", resultTexture);
        computeShader.SetInt("gridSize", gridSize); 
        
    }
}
