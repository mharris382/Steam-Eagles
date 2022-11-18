using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
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
    
    
    public struct DisconnectActionInfo
    {
        public Vector3Int cellPosition;
        public Tilemap tilemap;
        public TilemapTypes tilemapType;

        public DisconnectActionInfo(Tilemap tilemap, Vector3Int cellPosition)
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

        public override string ToString() => $"Removed cell at {cellPosition} on Tilemap:{tilemap.name}";
    }

    public struct SpawnedDynamicObjectInfo
    {
        public readonly ScriptableObject blockID;
        public GameObject dynamicBlock;

        public SpawnedDynamicObjectInfo(ScriptableObject blockID, GameObject dynamicBlock)
        {
            this.blockID = blockID;
            this.dynamicBlock = dynamicBlock;
        }
    }
    
}