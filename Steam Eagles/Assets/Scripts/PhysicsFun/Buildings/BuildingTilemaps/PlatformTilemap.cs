using Buildings.BuildingTilemaps;
using World;

namespace Buildings
{
    public class PlatformTilemap : EditableTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.PLATFORM;
        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - SolidTilemap.ORDER_IN_LAYER - 1;
        }
    }
}