using System;
using Spaces;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;


#endif
[CreateAssetMenu(menuName = "Steam Eagles/Block Database")]
public class BlockDatabase : ScriptableObject
{
    public Block[] blocks;
    
    [Serializable]
    public class Block
    {
        public string name;
        public BlockTilemap tilemap;
        public TileBase tile;
        public DynamicBlock dynamicBlock;
    }
}

public enum BlockTilemap
{
    PIPE,
    SOLID
}
