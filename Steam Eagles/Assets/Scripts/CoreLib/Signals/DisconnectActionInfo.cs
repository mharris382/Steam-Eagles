using System;
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
        
        [Obsolete("Use BuildingLayers instead")]
        public TilemapTypes tilemapType;
        
        public TileBase tile;
        public DisconnectActionInfo(Tilemap tilemap, Vector3Int cellPosition, TileBase tile=null)
        {
            this.cellPosition = cellPosition;
            this.tilemap = tilemap;
            this.tile = tile ? tile : (tilemap.GetTile(cellPosition));
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