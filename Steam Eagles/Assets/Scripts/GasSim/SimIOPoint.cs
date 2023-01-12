using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

public class SimIOPoint : MonoBehaviour
{
    
    [SerializeField] private Rect ioArea = new Rect(0, 0, 1, 1);
    
    
    public Rect IOArea => ioArea;


    public IEnumerable<Vector2Int> GetCellsFromRect(Grid grid, BoundsInt simBounds)
    {
        var pos = transform.TransformPoint(ioArea.position);
        var size = ioArea.size;
        var rect = new Rect(pos, size);
        var min = rect.min;
        var max = rect.max;
        var cellMin = grid.WorldToCell(min);
        var cellMax = grid.WorldToCell(max);
        cellMax.x = Mathf.Min(cellMax.x, simBounds.xMax);
        cellMax.y = Mathf.Min(cellMax.y, simBounds.yMax);
        cellMin.x = Mathf.Max(cellMin.x, simBounds.xMin);
        cellMin.y = Mathf.Max(cellMin.y, simBounds.yMin);
        for (var x = cellMin.x; x <= cellMax.x; x++)
        {
            for (var y = cellMin.y; y <= cellMax.y; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    protected virtual Color GizmoColor => Color.magenta;
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor.SetAlpha(0.5f);
        Gizmos.DrawCube(transform.TransformPoint(ioArea.position), ioArea.size);
    }
}

