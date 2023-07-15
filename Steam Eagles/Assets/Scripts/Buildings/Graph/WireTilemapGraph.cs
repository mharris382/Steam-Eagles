using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Buildings.Graph
{
    public class WireTilemapGraph : PowerTilemapGraph
    {
        private readonly BuildingPowerGrid _powerGrid;

        public override BuildingLayers Layers => BuildingLayers.WIRES;
        public override void OnTileAdded(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Added: {tile}");
        }

        public override void OnTileRemoved(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Removed: {tile}");
        }

        public WireTilemapGraph(Building building, BuildingPowerGrid powerGrid, CoroutineCaller caller, PowerConfig config) : base(building, powerGrid, caller, config)
        {
            _powerGrid = powerGrid;
        }

        protected override void HandleSupplyDeficit(List<IPowerSupplier> suppliers, List<IPowerConsumer> consumers, float supplyTotal, float demandTotal)
        {
            Debug.Log($"Wire Supply Deficit: {supplyTotal} < {demandTotal}");
        }


        
        public IEnumerable<BuildingCell> GetSourceCells()
        {
            IEnumerable<(BuildingCell, IPowerSupplier)> allSuppliers = _powerGrid.GetSuppliers();
            return allSuppliers
                .Where(t => t.Item2 != null && t.Item2.GetSupplyRate() > 0)
                .Select(t => t.Item1);
        }
    }
}