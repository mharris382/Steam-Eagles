using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Obsolete("use spaces.BlockDatabase")]
public class BlockData : ScriptableObject
{
    public string id;
    public TileBase blockStaticTile;
    public DynamicBlock blockDynamicPrefab;
}