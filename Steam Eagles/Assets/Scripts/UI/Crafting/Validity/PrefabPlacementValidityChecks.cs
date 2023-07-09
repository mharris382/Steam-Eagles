using System;
using Buildables;
using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting
{
    public class PrefabPlacementValidityChecks : PlacementValidityChecks<GameObject>
    {
        private readonly CraftingDirectionHandler _directionHandler;

        public PrefabPlacementValidityChecks(CraftingDirectionHandler directionHandler)
        {
            _directionHandler = directionHandler;
        }
        public override bool IsPlacementValid(Recipe recipe, GameObject loadedObject, GameObject character, Building building,
            BuildingCell cell, ref string invalidReason)
        {
            var bMachines = building.GetComponent<BMachines>();
            var buildable = loadedObject.GetComponent<BuildableMachineBase>();
            buildable.IsFlipped = _directionHandler.IsFlipped;
            return bMachines.Map.CanPlaceMachine(buildable, cell.cell2D, _directionHandler.IsFlipped, ref invalidReason);
        }
    }
}