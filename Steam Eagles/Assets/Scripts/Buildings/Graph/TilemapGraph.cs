using System;
using System.Collections.Generic;
using System.Diagnostics;
using Buildings.BuildingTilemaps;
using QuikGraph;

using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using QuikGraph.Algorithms;
namespace PhysicsFun.Buildings.Graph
{
    public abstract class TilemapGraph
    {
        private AdjacencyGraph<Vector3Int,Edge<Vector3Int>> graph;
        private readonly BuildingTilemap _tilemap;
        
        private int stronglyConnectedComponents = 0;
        IDictionary<Vector3Int, int> scComponents = new Dictionary<Vector3Int, int>();
        
        public int VertexCount => graph.VertexCount;


        public IEnumerable<Vector3Int> GetAllVertices() => graph.Vertices;

        public TilemapGraph(BuildingTilemap tilemap)
        {
            void FindEdges(Dictionary<Vector3Int, TileBase> dictionary, Stopwatch timer1)
            {
                Vector3Int[] neighborDirections = new Vector3Int[4]
                    { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

              
                foreach (var nonEmptyTile in dictionary)
                {
                    foreach (var neighborDirection in neighborDirections)
                    {
                        var neighborPosition = nonEmptyTile.Key + neighborDirection;
                        if (dictionary.ContainsKey(neighborPosition))
                        {
                            this.graph.AddEdge(new Edge<Vector3Int>(nonEmptyTile.Key, neighborPosition));
                        }
                    }
                }

                timer1.Stop();
                Debug.Log(
                    $"{tilemap.name}: Found {this.graph.EdgeCount} Edges in graph with {dictionary.Count} nodes non-empty tiles in {timer1.ElapsedMilliseconds} ms");
            }
            Dictionary<Vector3Int, TileBase> FindAllNonEmptyTiles(BoundsInt boundsInt, Tilemap tm1, Stopwatch stopwatch)
            {
                Dictionary<Vector3Int, TileBase> tileBases = new Dictionary<Vector3Int, TileBase>();
                for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
                {
                    for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
                    {
                        for (int z = boundsInt.zMin; z < boundsInt.zMax; z++)
                        {
                            var position = new Vector3Int(x, y, z);
                            var tile = tm1.GetTile(position);
                            if (tile != null) tileBases.Add(position, tile);
                        }
                    }
                }

                Debug.Log($"{tilemap.name}: Found {tileBases.Count} non-empty tiles in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Reset();
                return tileBases;
            }

            _tilemap = tilemap;
            var tm = tilemap.Tilemap;
            var bounds = tm.cellBounds;

            var timer = new Stopwatch();
            timer.Start();
            
            //find all non-empty tiles
            var nonEmptyTiles = FindAllNonEmptyTiles(bounds, tm, timer);
            
            
            //init adjacency graph
            this.graph = new AdjacencyGraph<Vector3Int, Edge<Vector3Int>>();
            this.graph.AddVertexRange(nonEmptyTiles.Keys);
            
            FindEdges(nonEmptyTiles, timer);
        }


        public int GetStronglyConnectedComponents(out IDictionary<Vector3Int, int> stronglyConnected)
        {
            scComponents = new Dictionary<Vector3Int, int>();
            stronglyConnectedComponents = graph.StronglyConnectedComponents(scComponents);
            stronglyConnected = scComponents;
            return stronglyConnectedComponents;
        }
        void CountStronglyConnectedComponents()
        {
            IDictionary<Vector3Int, int> components = new Dictionary<Vector3Int, int>();
            int componentCount = graph.StronglyConnectedComponents(components);
            Debug.Log($"Graph of {_tilemap.name} has {componentCount} strongly connected components");
            
        }
    }
}