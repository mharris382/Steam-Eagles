using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    /// <summary>
    /// class responsible for actually managing the gas simulation.
    /// This class has no knowledge of world space or transforms, it is only concerned
    /// ith the gas grid in cell space.  (calling classes are responsible for converting
    /// between world space and cell space).  
    /// </summary>
    public class GasSimGrid
    {
        private readonly Vector2Int chunkSize;
        private readonly Grid _grid;

        private Vector2Int _simulationSize;
        private Vector2Int _chunkCount;


        /// <summary>
        /// chunk size in terms of grid cells
        /// </summary>
        public Vector2Int ChunkSize => chunkSize;


        /// <summary>
        /// total size of the simulation in cells
        /// </summary>
        public Vector2Int SimulationSize => _simulationSize;


        /// <summary>
        /// number of chunks in the simulation
        /// </summary>
        public Vector2Int ChunkCount => _chunkCount;

        public int TotalCells => _simulationSize.x * _simulationSize.y;

        public GasSimGrid(Vector2Int chunkSize)
        {
            this.chunkSize = chunkSize;
            _simulationSize = Vector2Int.zero;
        }

        public GasSimGrid(Vector2Int chunkSize, Vector2Int simulationSize)
        {
            this.chunkSize = chunkSize;
            ResizeSimulation(simulationSize);
        }

        internal void ResizeSimulation(Vector2Int size)
        {

            _chunkCount = new Vector2Int(Mathf.CeilToInt(size.x / (float)chunkSize.x),
                Mathf.CeilToInt(size.y / (float)chunkSize.y));
            _simulationSize = new Vector2Int(_chunkCount.x * chunkSize.x, _chunkCount.y * chunkSize.y);
        }

        public IEnumerable<GasSimChunk> GetChunks()
        {
            for (int x = 0; x < _chunkCount.x; x++)
            {
                for (int y = 0; y < _chunkCount.y; y++)
                {
                    yield return  GetChunk(x, y);
                }
            }
        }

        public GasSimChunk GetChunk(int chunkX, int chunkY) => new GasSimChunk(new Vector2Int(chunkX,chunkY), new Vector2Int( chunkX * ChunkSize.x, chunkY * ChunkSize.y), chunkSize);
        public GasSimChunk GetCellChunk(int cellX, int cellY) => GetCellChunk(new Vector2Int(cellX, cellY));
        public GasSimChunk GetCellChunk(Vector2Int cell)
        {
            var chunkIndex = GetCellChunkIndex(cell);
            return GetChunk(chunkIndex.x, chunkIndex.y);
        }
        public Vector2Int GetCellChunkIndex(int cellX, int cellY) => new Vector2Int(cellX / ChunkSize.x, cellY / ChunkSize.y);
        public Vector2Int GetCellChunkIndex(Vector2Int cellCoord) => GetCellChunkIndex(cellCoord.x, cellCoord.y);
    }

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