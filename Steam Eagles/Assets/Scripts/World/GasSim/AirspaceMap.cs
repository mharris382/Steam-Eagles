using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using GasSim.SimCore.DataStructures;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class AirspaceMap : MonoBehaviour
{
    public SharedTilemap solidTilemap;

    [SerializeField]
    private Tilemap tilemap;

    public Rect[] searchAreas = new Rect[]
    {
        new Rect(-100, -100, 200, 200)
    };
    
    private void Awake()
    {
        solidTilemap.onValueChanged.AddListener(InitializeMapFromTilemap);   
    }
    
    void InitializeMapFromTilemap(Tilemap tilemap)
    {
        if (tilemap == null) return;
        var bounds = tilemap.cellBounds;
       
    }

    enum DrawMode
    {
        OFF,
        CellSpace,
        WorldSpace,
        LocalSpace
    }

    [SerializeField] private DrawMode mode;

    public IEnumerable<Vector3Int> GetCellsFromRect(Rect rect)
    {
        var cellMin = tilemap.layoutGrid.WorldToCell(rect.min);
        var cellMax = tilemap.layoutGrid.WorldToCell(rect.max);
        for (int x = cellMin.x; x <= cellMax.x; x++)
        {
            for (int y = cellMin.y; y <= cellMax.y; y++)
            {
                yield return new Vector3Int(x, y);
            }
        }
    }

    public Vector3Int[] cellsFound;
    
    private void OnDrawGizmos()
    {
        var color = Color.blue;
        
        if (tilemap == null) return;
        Gizmos.color = color;
        var bounds = tilemap.cellBounds;
        var min = bounds.min;
        var max = bounds.max;
        var center = bounds.center;
        

        Gizmos.color = Color.yellow;
        for (int i = 0; i < searchAreas.Length; i++)
        {
            searchAreas[i].DrawGizmos();
        }

        if(mode == DrawMode.OFF)    return;
        if (cellsFound.Length > 0)
        {
            Vector3 size = tilemap.cellSize * new Vector2(tilemap.transform.localScale.x ,tilemap.transform.localScale.y); 
            foreach (var vector3Int in cellsFound)
            {
                bool isSolid = tilemap.HasTile(vector3Int);
                color =  isSolid ? Color.Lerp(Color.red, Color.black, .5f) : Color.cyan;
                color.a /= 0.5f;
                Gizmos.color = color;
                Vector3 pos = vector3Int;
                switch (mode)
                {
                    case DrawMode.WorldSpace:
                        pos = tilemap.layoutGrid.CellToWorld(vector3Int);
                        break;
                    case DrawMode.LocalSpace:
                        pos = tilemap.layoutGrid.CellToLocal(vector3Int);
                        break;
                }
                if(!isSolid)
                    Gizmos.DrawWireCube(pos, size);
                else
                    Gizmos.DrawCube(pos, size);
                
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AirspaceMap))]
public class AirspaceMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var airspaceMap = target as AirspaceMap;
        if (GUILayout.Button("Dry Run"))
        {
            var rects = airspaceMap.searchAreas;
            airspaceMap.cellsFound = rects.SelectMany(t => airspaceMap.GetCellsFromRect(t)).ToArray();
        }
        base.OnInspectorGUI();
    }
}
#endif