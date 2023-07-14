using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Buildings.BuildingTilemaps;
using QuikGraph;
using QuikGraph.Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace Buildings.Graph
{
    public readonly struct Cell
    {
        public Vector2Int CellPosition { get; }
        public Cell(Vector2Int cellPosition)
        {
            CellPosition = cellPosition;
        }
        public Cell(Vector3Int cellPosition)
        {
            CellPosition = (Vector2Int)cellPosition;
        }
        
        private static Vector2Int[] neighborDirections = new Vector2Int[4]
            { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };


        public IEnumerable<Vector3Int> GetNeighbors()
        {
            foreach (var neighborDirection in neighborDirections)
            {
                yield return (Vector3Int)(CellPosition + neighborDirection);
            }
        }
    }

    public abstract class TilemapGraph
    {
        protected AdjacencyGraph<Vector3Int,Edge<Vector3Int>> graph;
        protected readonly BuildingTilemap _tilemap;
        
        protected int stronglyConnectedComponents = 0;
        protected IDictionary<Vector3Int, int> scComponents = new Dictionary<Vector3Int, int>();
        
        public int VertexCount => graph.VertexCount;


        public IEnumerable<Vector3Int> GetAllVertices() => graph.Vertices;

        void FindEdges(Dictionary<Vector3Int, TileBase> dictionary, Stopwatch timer1)
        {
            foreach (var nonEmptyTile in dictionary)
            {
                foreach (var neighborPosition in new Cell(nonEmptyTile.Key).GetNeighbors())
                {
                    if (dictionary.ContainsKey(neighborPosition))
                    {
                        this.graph.AddEdge(new Edge<Vector3Int>(nonEmptyTile.Key, neighborPosition));
                    }
                }
            }

            
        }
        
        Dictionary<Vector3Int, TileBase> FindAllNonEmptyTiles(BoundsInt boundsInt, Tilemap tm1)
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

            return tileBases;
        }
        public TilemapGraph(BuildingTilemap tilemap)
        {
            _tilemap = tilemap;
            var tm = tilemap.Tilemap;
            var bounds = tm.cellBounds;

            var timer = new Stopwatch();
            timer.Start();
            
            //find all non-empty tiles
            var nonEmptyTiles = FindAllNonEmptyTiles(bounds, tm);
            
            timer.Stop();
          
            //init adjacency graph
            this.graph = new AdjacencyGraph<Vector3Int, Edge<Vector3Int>>();
            this.graph.AddVertexRange(nonEmptyTiles.Keys);
            
            FindEdges(nonEmptyTiles, timer);
        }

        [System.Obsolete]
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