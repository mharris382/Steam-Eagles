using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Block Data", fileName = "New Block")]
public class BlockData : ScriptableObject
{
    public string id;
    public TileBase blockStaticTile;
    public DynamicBlock blockDynamicPrefab;
}