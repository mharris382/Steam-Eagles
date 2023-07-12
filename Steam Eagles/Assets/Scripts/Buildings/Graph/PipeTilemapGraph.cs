using Buildings.Rooms;
using QuikGraph;
using UnityEngine;

namespace Buildings.Graph
{
    public class PipeTilemapGraph : BuildingTilemapGraph
    {
        // AdjacencyGraph<BuildingTile, SEdge<BuildingTile>> _graph = new();
        
        public override BuildingLayers Layers => BuildingLayers.PIPE;
        public override void OnTileAdded(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Added: {tile}");
            // _graph.AddVertex(tile);
            // foreach (var buildingCell in tile.cell.GetNeighbors())
            // {
            //     
            // }
            // Debug.Assert(_graph.ContainsVertex(tile));
        }

        public override void OnTileRemoved(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Removed: {tile}");
        }

        public override void OnEdgeAdded(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Added: {edge.ToString()}");
        }

        public override void OnEdgeRemoved(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Removed: {edge.ToString()}");
        }

        public PipeTilemapGraph(Building building) : base(building)
        {
        }
    }
}