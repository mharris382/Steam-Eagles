using System.Collections.Generic;
using UnityEngine;

namespace Buildings.Rooms
{
    public class BoundsLookup
    {
        private readonly Room _room;
        private readonly Dictionary<BuildingLayers, BoundsInt> _boundsCache = new Dictionary<BuildingLayers, BoundsInt>();
        public BoundsLookup(Room room)
        {
            _room = room;
        }

        public BoundsInt GetBounds(BuildingLayers layer)
        {
            if (!_boundsCache.TryGetValue(layer, out var boundsInt))
            {
                var map = _room.Building.Map;
                _boundsCache.Add(layer, boundsInt = map.GetCellsForRoom(_room, layer));
            }
            return boundsInt;
        }
    }
}