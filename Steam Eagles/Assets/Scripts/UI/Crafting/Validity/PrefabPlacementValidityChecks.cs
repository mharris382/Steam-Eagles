using System;
using Buildables;
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
            var bMachines = building.GetComponent<BMachines>();
            var buildable = loadedObject.GetComponent<BuildableMachineBase>();
            return bMachines.Map.CanPlaceMachine(buildable, cell.cell2D, ref invalidReason);
        }
    }
}