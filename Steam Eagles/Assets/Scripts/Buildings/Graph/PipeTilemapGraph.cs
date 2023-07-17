using System.Collections.Generic;
using Buildings.Rooms;
using Cysharp.Threading.Tasks;
using QuikGraph;
using UnityEngine;
using Zenject;

namespace Buildings.Graph
{
    public class PipeTilemapGraph : PowerTilemapGraph
    {
        // AdjacencyGraph<BuildingTile, SEdge<BuildingTile>> _graph = new();
        
        
        

        public override BuildingLayers Layers => BuildingLayers.PIPE;
        public override void OnTileAdded(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Added: {tile}");
        }

        public override void OnTileRemoved(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Removed: {tile}");
        }

        protected override void HandleSupplyDeficit(List<IPowerSupplier> suppliers, List<IPowerConsumer> consumers, float supplyTotal, float demandTotal)
        {
            Debug.Log($"Pipe Supply Deficit: {supplyTotal} < {demandTotal}");
        }

        public override void OnEdgeAdded(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Added: {edge.ToString()}");
        }

        public override void OnEdgeRemoved(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Removed: {edge.ToString()}");
        }

        public PipeTilemapGraph(Building building, BuildingPowerGrid powerGrid, CoroutineCaller caller, PowerConfig config) : base(building, powerGrid, caller, config)
        {
        }
    }
}