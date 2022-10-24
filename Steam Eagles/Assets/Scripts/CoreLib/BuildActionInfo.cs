using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
    public struct BuildActionInfo
    {
        public Vector3Int cellPosition;
        public Tilemap tilemap;

        public BuildActionInfo(Tilemap tilemap, Vector3Int cellPosition)
        {
            this.cellPosition = cellPosition;
            this.tilemap = tilemap;
        }
        public override string ToString() => $"Added cell at {cellPosition} on Tilemap:{tilemap.name}";
    }
    
    
    public struct DisconnectActionInfo
    {
        public Vector3Int cellPosition;
        public Tilemap tilemap;

        public DisconnectActionInfo(Tilemap tilemap, Vector3Int cellPosition)
        {
            this.cellPosition = cellPosition;
            this.tilemap = tilemap;
        }

        public override string ToString() => $"Removed cell at {cellPosition} on Tilemap:{tilemap.name}";
    }

    public struct SpawnedDynamicObjectInfo
    {
        public GameObject dynamicBlock;

        public SpawnedDynamicObjectInfo(GameObject dynamicBlock)
        {
            this.dynamicBlock = dynamicBlock;
        }
    }
    
}