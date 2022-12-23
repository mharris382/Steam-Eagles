using World;

namespace Buildings.BuildingTilemaps
{
    public class PipeTilemap : EditableTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.PIPE;
        
        public override BuildingLayers GetBlockingLayers()
        {
            return base.GetBlockingLayers() | 
                   BuildingLayers.FOUNDATION | 
                   BuildingLayers.SOLID;
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - SolidTilemap.ORDER_IN_LAYER - 2;
        }
    }
}