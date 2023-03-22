using PhysicsFun.Buildings;
using World;

namespace Buildings.BuildingTilemaps
{
    public class WireTilemap : EditableTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.WIRES;
        
        public override string GetSaveID()
        {
            return "Wires";
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - 10;
        }
    }
}