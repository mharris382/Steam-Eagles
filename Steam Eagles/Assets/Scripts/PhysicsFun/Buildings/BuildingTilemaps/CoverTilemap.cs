using Buildings.BuildingTilemaps;
using PhysicsFun;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings
{
    [RequireComponent(typeof(WallTilemapFader))]
    [RequireComponent(typeof(TilemapRenderer))]
    public class CoverTilemap : RenderedTilemap
    {
                
        
        public override BuildingLayers Layer { get; }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer;
        }

        public override string GetSortingLayerName(Building building)
        {
            return "Near FG";
        }
    }
}