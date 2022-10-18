using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;


[Obsolete("Use TilemapEditTrigger instead")]
public class TilemapEditZone : TagTriggerArea
{
    public BoxCollider2D boxCollider;
    public Tilemap tilemap;
    public SharedTilemap tpEditableTilemap;
    public SharedTilemap bdEditableTilemap;

    protected override void OnTargetAdded(GameObject target, int totalNumberOfTargets)
    {
        OnPlayerEntered(target);
    }

    protected override void OnTargetRemoved(GameObject target, int totalNumberOfTargets)
    {
        OnPlayerExited(target);
    }

    private void OnPlayerEntered(GameObject player)
    {
        if (player.CompareTag("Transporter"))
        {
            tpEditableTilemap.Value = tilemap;
        }

        if (player.CompareTag("Builder"))
        {
            bdEditableTilemap.Value = tilemap;
        }
    }

    private void OnPlayerExited(GameObject player)
    {
        if (player.CompareTag("Transporter"))
        {
            if (tpEditableTilemap.Value == tilemap)
            {
                tpEditableTilemap.Value = null;
            }   
        }

        if (player.CompareTag("Builder"))
        {
            if (bdEditableTilemap.Value == tilemap)
            {
                bdEditableTilemap.Value = null;
            }
        }
    }
}


