using System;

namespace CoreLib
{

    
    /// <summary>
    /// enum referring to the different tilemaps which can be edited at runtime 
    /// </summary>
    [Obsolete("Use SteamEagles.Buildings.BuildingLayers instead")]
    public enum TilemapTypes
    {
        MODIFIER = 1,
        PIPE = 2,
        SOLIDS = 4,
        PIPE_MODIFIER = MODIFIER | PIPE,
        SOLIDS_MODIFIER = MODIFIER | SOLIDS,
    }
}