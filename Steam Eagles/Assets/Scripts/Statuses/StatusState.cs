using System;

namespace Statuses
{
    public enum StatusState
    {
        /// <summary>
        /// the entity does not have the status
        /// </summary>
        NONE = 0,
        /// <summary>
        /// the status is applied to the entity and is currently activated
        /// </summary>
        ACTIVE,
        /// <summary>
        /// the status is applied to the entity, but is either missing requirements or the status is blocked by another status
        /// </summary>
        DORMANT
    }

    [Flags]
    public enum EntityType
    {
        CHARACTER,
        VEHICLE,
        ALL = CHARACTER | VEHICLE
    }
}