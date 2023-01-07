using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    [Obsolete("use spaces.BlockDatabase")]
    public class BlockDatabase : ScriptableObject
    {
        
        [System.Serializable]
        public class Block
        {
            public TileBase tile;
        }
    }
}