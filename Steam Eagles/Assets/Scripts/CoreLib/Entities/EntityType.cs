using System;

namespace CoreLib.Entities
{
    [Flags]
    public enum EntityType
    {
        UNSPECIFIED,
        CHARACTER,
        ENEMY,
        VEHICLE,
        BUILDING,
        MACHINE,
        INVENTORY,
        PLAYER,
        PICKUP,
        PROJECTILE,
        STRUCTURE,
        TERRAIN,
        WEAPON,
        NPC,
        ALL = ~0
    }
}