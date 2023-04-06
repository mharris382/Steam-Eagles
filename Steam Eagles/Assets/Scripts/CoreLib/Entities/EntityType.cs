using System;

namespace Statuses
{
    [Flags]
    public enum EntityType
    {
        UNSPECIFIED,
        CHARACTER,
        VEHICLE,
        ALL = CHARACTER | VEHICLE
    }
}