using System;
using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting
{
    public class PrefabPlacementValidityChecks : PlacementValidityChecks<GameObject>
    {
        public override bool IsPlacementValid(Recipe recipe, GameObject loadedObject, GameObject character, Building building,
            BuildingCell cell, ref string invalidReason)
        {
            throw new NotImplementedException();
        }
    }
}