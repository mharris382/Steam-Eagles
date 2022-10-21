using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

public class CellHelper : MonoBehaviour
{
    [SerializeField]
    private SharedTilemap tilemap;

    
    /// <summary>
    /// get nearest cell coordinate in tilemap grid space
    /// </summary>
    public Vector3Int CellCoordinate
    {
        get => tilemap.Value.WorldToCell(transform.position);
    }

    /// <summary>
    /// center of cell in world space
    /// </summary>
    public Vector3 CellCenter
    {
        get => tilemap.Value.GetCellCenterWorld(CellCoordinate);
    }

    public bool HasTilemap => tilemap.HasValue;
    
    
    private void Awake()
    {
        tilemap.onValueChanged.AddListener(tm =>
        {
            enabled = tm != null;
        });
        enabled = tilemap.HasValue;
    }

    private void Update()
    {
        if (!tilemap.HasValue)
        {
            enabled = false;
            return;
        }
        
    }
}
