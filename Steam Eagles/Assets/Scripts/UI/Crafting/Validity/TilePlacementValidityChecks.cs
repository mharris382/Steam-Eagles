using System;
using System.Collections.Generic;
using Buildables;
using Buildings;
using Buildings.Tiles;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UI.Crafting
{
    public class TilePlacementValidityChecks : PlacementValidityChecks<TileBase>
    {
        private readonly OverlapValidityChecksConfig _config;
        private static List<Collider2D> _cache = new List<Collider2D>(10);

        public TilePlacementValidityChecks(OverlapValidityChecksConfig config)
        {
            _config = config;
        }
        public override bool IsPlacementValid(Recipe recipe, TileBase loadedObject, GameObject character, Building building, BuildingCell cell,
            ref string invalidReason)
        {
            if (_config.CheckOverlapForLayer(cell.layers))
            {
                bool overlapping = CheckIsOverlapping(building, cell, _config.GetContactFilterForOverlaps());
                if (overlapping)
                {
                    invalidReason = $"{_cache[0].name} is in the way";
                    return false;
                }
            }
            var bmachines = building.GetComponent<BMachines>();
            var bmachine = bmachines.Map.GetMachine(cell.cell2D);
            if (bmachine != null)
            {
                invalidReason = "Cannot place tile here";
                return false;
            }
            bmachine = bmachines.Map.GetMachine(cell.cell2D + Vector2Int.up);
            if (bmachine != null && bmachine.snapsToGround)
            {
                invalidReason = "Cannot remove tile";
                return false;
            }
            var t = building.Map.GetTile(cell);
            if (t != null && t is EditableTile)
            {
                return CheckPlacementValidity((EditableTile)t, recipe, loadedObject, character, building, cell, ref invalidReason);
            }
            return AllowPlacementOnOtherTiles(cell.layers) || t == null;
        }

        public bool AllowPlacementOnOtherTiles(BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.COVER:
                case BuildingLayers.WALL:
                case BuildingLayers.SOLID:
                case BuildingLayers.PIPE:
                case BuildingLayers.PLATFORM:
                case BuildingLayers.DECOR:
                case BuildingLayers.WIRES:
                case BuildingLayers.LADDERS:
                    return true;
                case BuildingLayers.GAS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
            return true;
        }

        private bool CheckPlacementValidity(EditableTile tileFound, Recipe recipe, TileBase loadedObject, GameObject character, Building building, BuildingCell cell, ref string invalidReason)
        {
            var layer = tileFound.GetLayer();
            switch (layer)
            {
                case BuildingLayers.NONE:
                    break;
                case BuildingLayers.WALL:
                    break;
                case BuildingLayers.SOLID:
                case BuildingLayers.FOUNDATION:
                    break;
                case BuildingLayers.LADDERS:
                    break;
                case BuildingLayers.PIPE:
                    break;
                case BuildingLayers.COVER:
                    break;
                case BuildingLayers.PLATFORM:
                    break;
                case BuildingLayers.DECOR:
                    break;
                case BuildingLayers.WIRES:
                    break;
                case BuildingLayers.GAS:
                    break;
                case BuildingLayers.REQUIRED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        static bool CheckIsOverlapping(Building building, BuildingCell cell, ContactFilter2D contactFilter)
        {
            var cellSize = (Vector3)building.Map.GetCellSize(cell.layers);
            var wsPosMin = building.Map.CellToWorld(cell);
            var wsPosMax = wsPosMin + cellSize;
            contactFilter.useTriggers = false;
            contactFilter.useDepth = false;
            return Physics2D.OverlapArea(wsPosMin, wsPosMax, contactFilter, _cache) > 0;
        }
    }
}