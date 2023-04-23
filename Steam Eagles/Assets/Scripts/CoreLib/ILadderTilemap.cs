using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
    public interface ILadderTilemap
    {
        
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }
        
        
        Tilemap Tilemap { get; }
    }
}