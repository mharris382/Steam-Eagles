using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting
{
    public abstract class PlacementValidityChecks<T> : PlacementValidityChecks where T : Object
    {
        public override bool IsPlacementValid(Recipe recipe, Object loadedObject, GameObject character, Building building, BuildingCell cell, ref string invalidReason)
        {
            return IsPlacementValid(recipe, (T) loadedObject, character, building, cell, ref invalidReason);
        }

        public abstract bool IsPlacementValid(Recipe recipe, T loadedObject, GameObject character, Building building, BuildingCell cell, ref string invalidReason);
    }

    public abstract class PlacementValidityChecks
    {
        public abstract bool IsPlacementValid(Recipe recipe,
            Object loadedObject, GameObject character, Building building,
            BuildingCell cell, ref string invalidReason);
    }
}