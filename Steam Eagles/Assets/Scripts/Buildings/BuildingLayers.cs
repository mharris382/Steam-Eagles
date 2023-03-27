using System;

namespace World
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
    
    
}