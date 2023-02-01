using Buildings.BuildingTilemaps;
using PhysicsFun.Buildings;
using World;

namespace Buildings
{
    public class DecorTilemap : EditableTilemap
    {
        
        public override BuildingLayers Layer => BuildingLayers.DECOR;
        public override string GetSaveID()
        {
            return "Decor";
        }

        public override BuildingLayers GetBlockingLayers()
        {
            return BuildingLayers.SOLID | BuildingLayers.DECOR | BuildingLayers.PIPE;
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - (SolidTilemap.ORDER_IN_LAYER - 1);
        }
    }
}