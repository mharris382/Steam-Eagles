using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Steam Eagles/Static Block")]
public class StaticBlock : ScriptableObject
{
    public BlockMapType targetMap;
    
    public TileBase tile;
    public Color color;
    public Sprite sprite;

    public static implicit operator TileBase(StaticBlock block)
    {
        return block.tile;
    }
}