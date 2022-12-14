using Buildings;
using Buildings.BuildingTilemaps;

namespace PhysicsFun.Buildings
{
    public interface IStructure
    {
        FoundationTilemap FoundationTilemap { get; }
        SolidTilemap SolidTilemap { get; }
        PipeTilemap PipeTilemap { get; }
        WallTilemap WallTilemap { get; }
    }
}