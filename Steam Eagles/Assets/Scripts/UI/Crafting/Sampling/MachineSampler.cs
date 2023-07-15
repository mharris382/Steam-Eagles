using Buildables;
using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting.Sampling
{
    [SampleOrder(-10)]
    public class MachineSampler : ISampler
    {
        public bool TryGetRecipe(Building building, Vector3 aimPosition, out Recipe recipe)
        {
            recipe = null;
            var machineCell = building.Map.WorldToCell(aimPosition, BuildingLayers.SOLID);
            var machines = building.GetComponent<BMachines>() ?? building.gameObject.AddComponent<BMachines>();
            if (machines == null) return false;
            if(machines.Map.TryGetMachine(machineCell, out var machine))
            {
                var recipeInstance = machine.GetComponent<RecipeInstance>();
                if (!recipeInstance.IsLoaded)
                    return false;
                recipe = recipeInstance.Recipe;
                return true;
            }
            return false;
        }
    }
}