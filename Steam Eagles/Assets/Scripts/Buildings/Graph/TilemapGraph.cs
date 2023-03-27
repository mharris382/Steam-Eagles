using System;
using System.Collections.Generic;
using System.Diagnostics;
using Buildings.BuildingTilemaps;
using QuikGraph;

using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace PhysicsFun.Buildings.Graph
{
    public abstract class TilemapGraph
    {
        private readonly AdjacencyGraph<Vector3Int,Edge<Vector3Int>> graph;

        public TilemapGraph(BuildingTilemap tilemap)
        {
            var tm = tilemap.Tilemap;
            var bounds = tm.cellBounds;
            
            var timer = new Stopwatch();
            timer.Start();
            
            //find all non-empty tiles
            Dictionary<Vector3Int, TileBase> nonEmptyTiles = new Dictionary<Vector3Int, TileBase>();
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (int z = bounds.zMin; z < bounds.zMax; z++)
                    {
                        var position = new Vector3Int(x, y, z);
                        var tile = tm.GetTile(position);
                        if (tile != null) nonEmptyTiles.Add(position, tile);
                    }   
                }
            }
            
            Debug.Log($"{tilemap.name}: Found {nonEmptyTiles.Count} non-empty tiles in {timer.ElapsedMilliseconds} ms");
            timer.Reset();

            Vector3Int[] neighborDirections = new Vector3Int[4] { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

#if false

            //create graph by checking neighbors of each non-empty tile and adding edges
            Dictionary<Vector3Int, Vector3Int[]> neighborDict = new Dictionary<Vector3Int, Vector3Int[]>();
            List<Vector3Int> neighbors = new List<Vector3Int>();
            int edgeCount = 0;
            foreach (var nonEmptyTile in nonEmptyTiles)
            {
                neighbors.Clear();
                foreach (var neighborDirection in neighborDirections)
                {
                    var neighborPosition = nonEmptyTile.Key + neighborDirection;
                    if (nonEmptyTiles.ContainsKey(neighborPosition))
                    {
                        neighbors.Add(neighborPosition);
                        edgeCount++;
                    }
                }
                neighborDict.Add(nonEmptyTile.Key, neighbors.ToArray());
            }

            timer.Stop();
            Debug.Log($"Found {edgeCount}({edgeCount/2}) Edges in graph with {nonEmptyTiles.Count} nodes non-empty tiles in {timer.ElapsedMilliseconds} ms");
            
            //wrap dictionary in adjacency graph
            this.graph = neighborDict.ToDelegateVertexAndEdgeListGraph(kv => Array.ConvertAll(kv.Value, v => new Edge<Vector3Int>(kv.Key, v)));
            
#endif
            this.graph = new AdjacencyGraph<Vector3Int, Edge<Vector3Int>>();
            this.graph.AddVertexRange(nonEmptyTiles.Keys);
            foreach (var nonEmptyTile in nonEmptyTiles)
            {
                foreach (var neighborDirection in neighborDirections)
                {
                    var neighborPosition = nonEmptyTile.Key + neighborDirection;
                    if (nonEmptyTiles.ContainsKey(neighborPosition))
                    {
                        this.graph.AddEdge(new Edge<Vector3Int>(nonEmptyTile.Key, neighborPosition));
                    }
                }
            }
            timer.Stop();
            Debug.Log($"{tilemap.name}: Found {this.graph.EdgeCount} Edges in graph with {nonEmptyTiles.Count} nodes non-empty tiles in {timer.ElapsedMilliseconds} ms");
        }
    }
}