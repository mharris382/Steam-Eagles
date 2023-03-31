using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;

namespace Buildings
{
    [Flags]
    public enum BuildingLayers
    {
        NONE,
        WALL=1,
        FOUNDATION=2,
        SOLID=4,
        PIPE=8,
        COVER=16,
        PLATFORM=32,
        DECOR=64,
        WIRES=128,
        LADDERS=256,
        REQUIRED = FOUNDATION | SOLID | PIPE | WIRES,
    }


    public static class BuildingLayersExtensions
    {
        public static Type GetBuildingTilemapType(this BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.NONE:
                    return null;
                    break;
                case BuildingLayers.WALL:
                    return typeof(WallTilemap);
                    break;
                case BuildingLayers.FOUNDATION:
                    return typeof(FoundationTilemap);
                    break;
                case BuildingLayers.SOLID:
                    return typeof(SolidTilemap);
                    break;
                case BuildingLayers.PIPE:
                    return typeof(PipeTilemap);
                    break;
                case BuildingLayers.COVER:
                    return typeof(CoverTilemap);
                    break;
                case BuildingLayers.PLATFORM:
                    return typeof(PlatformTilemap);
                    break;
                case BuildingLayers.DECOR:
                    return typeof(DecorTilemap);
                    break;
                case BuildingLayers.WIRES:
                    return typeof(WireTilemap);
                    break;
                case BuildingLayers.LADDERS:
                    return typeof(LadderTilemap);
                    break;
                case BuildingLayers.REQUIRED:
                    throw new InvalidOperationException("Cannot request a type for a layer that is not a single layer.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }

        public static IEnumerable<Type> GetBuildingTilemapTypes(this BuildingLayers layers)
        {
            foreach (var value in Enum.GetValues(typeof(BuildingLayers)))
            {
                BuildingLayers l = value as BuildingLayers? ?? BuildingLayers.NONE;
                if(l == BuildingLayers.NONE || l == BuildingLayers.REQUIRED)
                    continue;
                if (layers.HasFlag(l))
                {
                    yield return l.GetBuildingTilemapType();
                }
            }
        }
    }
}