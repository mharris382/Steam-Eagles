using Buildings.BuildingTilemaps;
using World;

namespace Buildings
{
    public class FoundationTilemap : RenderedTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.FOUNDATION;
        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer + SolidTilemap.ORDER_IN_LAYER + 1;
        }
    }
}