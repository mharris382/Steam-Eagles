using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    public struct GasSimChunk
    {
        public Vector2Int ChunkIndex { get; }
        public Vector2Int ChunkPosition { get; }
        public Vector2Int ChunkSize { get; }
        public RectInt ChunkBounds { get; }
        public GasSimChunk(Vector2Int chunkIndex, Vector2Int chunkPosition, Vector2Int chunkSize)
        {
            ChunkIndex = chunkIndex;
            ChunkPosition = chunkPosition;
            ChunkSize = chunkSize;
            ChunkBounds = new RectInt(chunkPosition, chunkSize);
        }
        
        public IEnumerable<Vector2Int> GetChunkCells()
        {
            for (var x = 0; x < ChunkSize.x; x++)
            {
                for (var y = 0; y < ChunkSize.y; y++)
                {
                    yield return ChunkPosition + new Vector2Int(x, y);
                }
            }
        }
    }
}