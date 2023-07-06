using System;
using Buildables;
using Buildings;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UI.Crafting
{
    public class TilePlacementValidityChecks : PlacementValidityChecks<TileBase>
    {
        public override bool IsPlacementValid(Recipe recipe, TileBase loadedObject, GameObject character, Building building, BuildingCell cell,
            ref string invalidReason)
        {
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
            return t == null;
        }
    }
}