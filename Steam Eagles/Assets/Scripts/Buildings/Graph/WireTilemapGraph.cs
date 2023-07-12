using UnityEngine;

namespace Buildings.Graph
{
    public class WireTilemapGraph : BuildingTilemapGraph
    {
        public WireTilemapGraph(Building building) : base(building)
        {
        }

        public override BuildingLayers Layers => BuildingLayers.WIRES;
        public override void OnTileAdded(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Added: {tile}");
        }

        public override void OnTileRemoved(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Removed: {tile}");
        }
    }
}