using System.Collections.Generic;
using System.Linq;
using Buildings.Graph;
using Buildings.Rooms;
using QuikGraph;
using UnityEngine;

namespace Buildings
{

    public class BuildingGraphs
    {
        private readonly BuildingMap _map;
        private Dictionary<Vector2, TilemapGraph> _graphs = new Dictionary<Vector2, TilemapGraph>();


        public BuildingGraphs(BuildingMap map)
        {
            _map = map;

            var rooms = _map.RoomGraph.Graph.Vertices.ToArray();
            foreach (var uniqueLayer in _map.GetUniqueLayers())
            {
                if (!_graphs.ContainsKey(uniqueLayer.cellSize))
                {
                    _graphs.Add(uniqueLayer.cellSize, new TilemapGraph(uniqueLayer.layer, _map, rooms));
                }
            }
            
        }

        public TilemapGraph GetGraphForLayer(BuildingLayers layer)
        {
            var size = _map.GetCellSize(layer);
            return _graphs[size];
        }

        public class TilemapGraph
        {
            private readonly BuildingMap _map;
            private readonly BuildingLayers _layer;

            public BidirectionalMatrixGraph<TileEdge> Graph { get; }

            public AdjacencyGraph<Vector2Int, Edge<Vector2Int>> AdjacencyGraph { get; set; }

            public class TileEdge : Edge<Vector2Int>, IEdge<int>
            {
                int IEdge<int>.Source => base.Source.GetHashCode();
                int IEdge<int>.Target => base.Target.GetHashCode();
                public TileEdge(Vector2Int source, Vector2Int target) : base(source, target) { }
            }

            public TilemapGraph(BuildingLayers layers, BuildingMap map, IEnumerable<Room> rooms)
            {
                Debug.Assert(rooms != null);
                var cellSize = map.GetCellSize(layers);
                this._map = map;
                this._layer = layers;
                var bounds = rooms
                    .Where(t => t.buildLevel == BuildLevel.FULL)
                    .Select(t => t.GetCells(map.GetTilemap(layers).layoutGrid)).ToArray();
                
                var numberOfCells = bounds.Select(t => t.size.x * t.size.y).Sum();
                //BidirectionalMatrixGraph<TileEdge> graph = new BidirectionalMatrixGraph<TileEdge>(numberOfCells);
                AdjacencyGraph<Vector2Int, Edge<Vector2Int>> adjacencyGraph = new AdjacencyGraph<Vector2Int, Edge<Vector2Int>>();
                //find all cells
                HashSet<Vector3Int> allCells = new HashSet<Vector3Int>();
                foreach (var boundsInt in bounds)
                {
                    FindAllCellsInBounds(map, boundsInt, allCells);
                }
                Debug.Log($"Found {allCells.Count} cells on layer {layers}. Now creating dense graph.");
                //once all cells are hashed, check if neighbors exist. if so add edge
                var directions = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
                foreach (var cell in allCells)
                {
                    foreach (var direction in directions)
                    {
                        var neighbor = cell + direction;
                        if (allCells.Contains(neighbor))
                        {
                            //graph.AddEdge(new TileEdge((Vector2Int)cell, (Vector2Int)neighbor));
                            adjacencyGraph.AddVerticesAndEdge(new Edge<Vector2Int>((Vector2Int)cell, (Vector2Int)neighbor));
                        }
                    }
                }
                
                //this.Graph = graph;
                this.AdjacencyGraph = adjacencyGraph;
                ///Debug.Log($"MATRIX GRAPH: Found {graph.VertexCount} vertices and {graph.EdgeCount} edges");
                Debug.Log($"ADJACENCY GRAPH: Found {adjacencyGraph.VertexCount} vertices and {adjacencyGraph.EdgeCount} edges");
            }

            private void FindAllCellsInBounds(BuildingMap map, BoundsInt boundsInt, HashSet<Vector3Int> allCells)
            {
                for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
                {
                    for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        
                        if (CheckIfCellHasFoundationTile(map, cell))
                            continue;

                        if (allCells.Contains(cell))
                            Debug.LogWarning("Cell found in multiple bounds");
                        else
                            allCells.Add(cell);
                    }
                }
            }

            private bool CheckIfCellHasFoundationTile(BuildingMap map, Vector3Int cell)
            {
                var foundationCells = map.ConvertBetweenLayers(this._layer, BuildingLayers.FOUNDATION, cell);
                foreach (var foundationCell in foundationCells)
                {
                    if (map.GetTile(foundationCell, BuildingLayers.FOUNDATION))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
}
    
    
}