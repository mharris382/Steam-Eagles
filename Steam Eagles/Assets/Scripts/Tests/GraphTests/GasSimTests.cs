using System.Collections.Generic;
using GasSim;
using NUnit.Framework;
using UnityEngine;

namespace Tests.GraphTests
{
    [TestFixture]
    public class GasSimTests
    {
        private GasSim.GasGridController _gasManager;
        
        
        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("GasSimManager");
            _gasManager = go.AddComponent<GasGridController>();
            
        }
        
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_gasManager.gameObject);
        }

        [Test]
        public void Simulation_Chunks_Grids_Correctly()
        {
            int expectedChunksX = 10;
            int expectedChunksY = 10;
            Vector2Int chunkSize = _gasManager.GasSimGrid.ChunkSize;
            Vector2Int chunkCount = _gasManager.GasSimGrid.ChunkCount;
            Assert.AreEqual(Vector2Int.zero, chunkCount);

            int sizeX = UnityEngine.Random.Range((chunkSize.x * (expectedChunksX - 1))+1, chunkSize.x * expectedChunksX);
            int sizeY = UnityEngine.Random.Range((chunkSize.y * (expectedChunksY - 1))+1, chunkSize.y * expectedChunksY);
            _gasManager.ResizeSimulation(Vector2Int.zero, new Vector2Int(sizeX, sizeY));
            chunkCount = _gasManager.GasSimGrid.ChunkCount;
            Assert.AreEqual(expectedChunksX, chunkCount.x);
            Assert.AreEqual(expectedChunksY, chunkCount.y);
            
            int expectedTotalCells = (expectedChunksX * chunkSize.x) * (expectedChunksY * chunkSize.y);
            Assert.AreEqual(expectedTotalCells, _gasManager.GasSimGrid.TotalCells);
        }


        [Test]
        public void Converts_From_World_To_Sim_Coordinates()
        {
            int startX = 1;
            int startY = 1;
            Vector2 worldPosition = new Vector2(startX, startY);
            Vector2Int simulationOffset = new Vector2Int(-10, -10);
            Vector2Int simulationSize = new Vector2Int(20, 20);
            _gasManager.ResizeSimulation(simulationOffset, simulationSize);
            Vector2Int gridPos = _gasManager.WorldToGridPosition(worldPosition);
            Vector2 expectedPosition = new Vector2(gridPos.x - simulationOffset.x, gridPos.y - simulationOffset.y);
            Vector2Int simPosition = _gasManager.WorldToSimPosition(worldPosition);
            
            Assert.AreEqual(expectedPosition, new Vector2(simPosition.x, simPosition.y));
        }
        
        [Test]
        public void Converts_From_Sim_To_World_Coordinates()
        {
            int startX = 1;
            int startY = 1;
            Vector2Int simPosition = new Vector2Int(startX, startY);
            Vector2Int simulationOffset = new Vector2Int(-10, -10);
            Vector2Int simulationSize = new Vector2Int(20, 20);
            _gasManager.ResizeSimulation(simulationOffset, simulationSize);
            
            Vector2 expectedPosition = new Vector2(
                (simulationOffset.x + startX) * _gasManager.CellSize.x,
                (simulationOffset.y + startY) * _gasManager.CellSize.y);
            
            
            Vector2 actualWorld = _gasManager.SimToWorldPosition(simPosition);
            
            Assert.AreEqual(expectedPosition, actualWorld);

            
        }
        
        
        [Test]
        public void Converts_From_Sim_To_Grid_Coordinates()
        {
            int startX = 1;
            int startY = 1;
            int expectX = -9;
            int expectY = -9;
            
            Vector2Int simulationOffset = new Vector2Int(-10, -10);
            Vector2Int simulationPosition = new Vector2Int(startX, startY);
            Vector2Int expectedGridPosition = new Vector2Int(simulationPosition.x + startX, simulationPosition.y + startY);
            Vector2Int simulationSize = new Vector2Int(20, 20);
            
            _gasManager.ResizeSimulation(simulationOffset, simulationSize);
            Vector2Int expectedPosition = new Vector2Int(simulationOffset.x + startX, simulationOffset.y + startY);
            Vector2Int simPosition = _gasManager.GridToSimPosition(expectedGridPosition);
            
            (Vector2Int simPosition, Vector2Int gridPosition)[] pairs =
                new (Vector2Int simPosition, Vector2Int gridPosition)[]
            {
                (Vector2Int.zero, simulationOffset),
                (Vector2Int.right, simulationOffset + Vector2Int.right),
                (Vector2Int.up, simulationOffset + Vector2Int.up),
                (Vector2Int.one, simulationOffset + Vector2Int.one),
                (Vector2Int.down, simulationOffset + Vector2Int.down),
                (Vector2Int.left, simulationOffset + Vector2Int.left),
                (Vector2Int.down + Vector2Int.left, simulationOffset + Vector2Int.down + Vector2Int.left)
            };
            foreach (var valueTuple in pairs)
            {
                var simPos = valueTuple.simPosition;
                var gridPos = valueTuple.gridPosition;
                Assert.AreEqual(gridPos, _gasManager.SimToGridPosition(simPos));
            }
        }
        
        [Test]
        public void Converts_From_Grid_To_Sim_Coordinates()
        {
            int startX = 1;
            int startY = 1;
            Vector2Int gridPosition = new Vector2Int(startX, startY);
            Vector2Int simulationOffset = new Vector2Int(-10, -10);
            Vector2Int simulationSize = new Vector2Int(20, 20);
            
            _gasManager.ResizeSimulation(simulationOffset, simulationSize);
            Vector2Int expectedPosition = new Vector2Int(simulationOffset.x + startX, simulationOffset.y + startY);
            Vector2Int simPosition = _gasManager.GridToSimPosition(gridPosition);
            
            (Vector2Int simPosition, Vector2Int gridPosition)[] pairs =
                new (Vector2Int simPosition, Vector2Int gridPosition)[]
                {
                    (Vector2Int.zero, simulationOffset),
                    (Vector2Int.right, simulationOffset + Vector2Int.right),
                    (Vector2Int.up, simulationOffset + Vector2Int.up),
                    (Vector2Int.one, simulationOffset + Vector2Int.one),
                    (Vector2Int.down, simulationOffset + Vector2Int.down),
                    (Vector2Int.left, simulationOffset + Vector2Int.left),
                    (Vector2Int.down + Vector2Int.left, simulationOffset + Vector2Int.down + Vector2Int.left)
                };

            foreach (var valueTuple in pairs)
            {
                var simPos = valueTuple.simPosition;
                var gridPos = valueTuple.gridPosition;
                Assert.AreEqual(simPos, _gasManager.GridToSimPosition(gridPos));
            }
        }


        public void GetChunk_Cells()
        {
            int expectedChunksX = 10;
            int expectedChunksY = 10;
            Vector2Int chunkSize = _gasManager.GasSimGrid.ChunkSize;
            int sizeX = chunkSize.x * expectedChunksX;
            int sizeY = chunkSize.y * expectedChunksY;
            _gasManager.ResizeSimulation(Vector2Int.zero, new Vector2Int(sizeX, sizeY));
            Vector2Int chunkCount = _gasManager.GasSimGrid.ChunkCount;
        }
        
        [Test]
        public void Cell_To_Chunk()
        {
            int expectedChunksX = 10;
            int expectedChunksY = 10;
            Vector2Int chunkSize = _gasManager.GasSimGrid.ChunkSize;
            
            Vector2Int chunkCount = _gasManager.GasSimGrid.ChunkCount;
            Assert.AreEqual(Vector2Int.zero, chunkCount);

            int sizeX = chunkSize.x * expectedChunksX;
            int sizeY = chunkSize.y * expectedChunksY;
            _gasManager.ResizeSimulation(Vector2Int.zero, new Vector2Int(sizeX, sizeY));
            chunkCount = _gasManager.GasSimGrid.ChunkCount;
            Assert.AreEqual(chunkCount, new Vector2Int(expectedChunksX, expectedChunksY));
            
            
            Assert.AreEqual(expectedChunksX, chunkCount.x);
            Assert.AreEqual(expectedChunksY, chunkCount.y);
            Debug.Log($"Chunk Count = {chunkCount}");
            var grid = _gasManager.GasSimGrid;
            
            int cellX = 0;
            int cellY = 0;
            int chunkX = 0;
            int chunkY = 0;
            int chunkCountX = grid.ChunkCount.x;
            int chunkCountY = grid.ChunkCount.y;
            int cellsPerChunk = chunkSize.x * chunkSize.y;
            
            for (chunkX = 0; chunkX < chunkCountX; chunkX++) 
            {
                for (chunkY = 0; chunkY < chunkCountY; chunkY++)
                {
                    var expectedChunk = new Vector2Int(chunkX, chunkY);
                    var chunkStart = new Vector2Int(chunkX * chunkSize.x, chunkY * chunkSize.y);
                    var chunk = grid.GetChunk(chunkX, chunkY);
                    Assert.AreEqual(chunkStart, chunk.ChunkPosition, GetFailureInfo());
                }    
            }

            var totalSize = grid.SimulationSize;
            Assert.AreEqual(expectedChunksX * chunkSize.x, totalSize.x);
            Assert.AreEqual(expectedChunksY * chunkSize.y, totalSize.y);
            
            var totalX = totalSize.x;
            var totalY = totalSize.y;
            for (int x = 0; x < totalX; x++)
            {
                for (int y = 0; y < totalY; y++)
                {
                    var expectedChunk = new Vector2Int(x / chunkSize.x, y / chunkSize.y);
                    var chunk = grid.GetCellChunk(x, y);
                    Assert.AreEqual(expectedChunk, chunk.ChunkIndex, GetFailureInfo2());
                    string GetFailureInfo2()
                    {
                        return $"Failed ON: Chunk ({expectedChunk.x}, {expectedChunk.y}) and Cell ({x}, {y})";
                    }
                }
            }
                
            string GetFailureInfo()
            {
                return $"Failed ON: Chunk ({chunkX}, {chunkY}) and Cell ({cellX}, {cellY})";
            }

        }


        [Test]
        public void Iterate_On_Chunks()
        {
            int expectedChunksX = 10;
            int expectedChunksY = 10;
            Vector2Int chunkSize = _gasManager.GasSimGrid.ChunkSize;
            
            Vector2Int chunkCount = _gasManager.GasSimGrid.ChunkCount;
            Assert.AreEqual(Vector2Int.zero, chunkCount);

            int sizeX = chunkSize.x * expectedChunksX;
            int sizeY = chunkSize.y * expectedChunksY;
            _gasManager.ResizeSimulation(Vector2Int.zero, new Vector2Int(sizeX, sizeY));
            var grid = _gasManager.GasSimGrid;
            HashSet<Vector2Int> foundCells = new HashSet<Vector2Int>();
            int totalCells = 0;
            foreach (var gasSimChunk in grid.GetChunks())
            {
                foreach (var chunkCell in gasSimChunk.GetChunkCells())
                {
                    Assert.IsFalse(foundCells.Contains(chunkCell));
                    foundCells.Add(chunkCell);
                    totalCells++;
                }
            }

            Assert.AreEqual(grid.TotalCells, totalCells);
        }
    }

}