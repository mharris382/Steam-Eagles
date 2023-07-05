using System;
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
            var t = building.Map.GetTile(cell);
            return t == null;
        }
    }
}