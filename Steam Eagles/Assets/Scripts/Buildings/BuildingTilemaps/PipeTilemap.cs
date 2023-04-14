using System.Collections.Generic;
using PhysicsFun.Buildings;
using World;

namespace Buildings.BuildingTilemaps
{
    public class PipeTilemap : DestructableTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.PIPE;
        public override string GetSaveID()
        {
            return "Pipe";
        }

        public override BuildingLayers GetBlockingLayers()
        {
            return base.GetBlockingLayers() | 
                   BuildingLayers.FOUNDATION | 
                   BuildingLayers.SOLID;
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - SolidTilemap.ORDER_IN_LAYER - 1;
        }

        public override IEnumerable<string> GetTileAddresses()
        {
            yield return "PipeTile";
        }
    }
}