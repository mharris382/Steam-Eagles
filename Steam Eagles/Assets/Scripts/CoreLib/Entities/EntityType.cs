using System;

namespace CoreLib.MyEntities
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
        STORM,
        PLAYER,
        PICKUP,
        PROJECTILE,
        STRUCTURE,
        TERRAIN,
        WEAPON,
        NPC,
        APPLIANCE,
        ALL = ~0
    }
}