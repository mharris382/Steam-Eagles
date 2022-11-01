using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    [CreateAssetMenu(fileName = "BlockDatabase", menuName = "BlockDatabase", order = 0)]
    public class BlockDatabase : ScriptableObject
    {
        
        [System.Serializable]
        public class Block
        {
            public TileBase tile;
        }
    }
}