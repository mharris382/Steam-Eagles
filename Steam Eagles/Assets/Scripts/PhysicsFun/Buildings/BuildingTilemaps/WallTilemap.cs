using Buildings.BuildingTilemaps;
using World;

namespace Buildings
{
    public class WallTilemap : BuildingTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.WALL;
    }
}