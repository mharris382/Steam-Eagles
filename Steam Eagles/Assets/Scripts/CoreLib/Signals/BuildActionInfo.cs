using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
    /// <summary>
    /// called when tile is placed on the map
    /// </summary>
    public struct BuildActionInfo
    {
        public Vector3Int cellPosition;
        public Tilemap tilemap;
        public TilemapTypes tilemapType;

        public BuildActionInfo(Tilemap tilemap, Vector3Int cellPosition)
        {
            this.cellPosition = cellPosition;
            this.tilemap = tilemap;
            if (tilemap.gameObject.CompareTag("Pipe Tilemap"))
            {
                tilemapType = TilemapTypes.PIPE;
            }
            else
            {
                tilemapType = TilemapTypes.SOLIDS;
            }
        }
        public override string ToString() => $"Added cell at {cellPosition} on Tilemap:{tilemap.name}";
    }
}