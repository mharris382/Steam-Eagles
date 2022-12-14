using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
    /// <summary>
    /// called when a block is disconnected from the tilemap (i.e. Tile removed)
    /// </summary>
    public struct DisconnectActionInfo
    {
        public Vector3 WorldPosition => tilemap.CellToWorld(cellPosition);
        public Vector3Int cellPosition;
        public Tilemap tilemap;
        public TilemapTypes tilemapType;
        public Tile tile;
        public DisconnectActionInfo(Tilemap tilemap, Vector3Int cellPosition)
        {
            this.cellPosition = cellPosition;
            this.tilemap = tilemap;
            tile = tilemap.GetTile(cellPosition) as Tile;
            if (tilemap.gameObject.CompareTag("Pipe Tilemap"))
            {
                tilemapType = TilemapTypes.PIPE;
            }
            else
            {
                tilemapType = TilemapTypes.SOLIDS;
            }
        }

        public override string ToString() => $"Removed cell at {cellPosition} on Tilemap:{tilemap.name}";
    }
}