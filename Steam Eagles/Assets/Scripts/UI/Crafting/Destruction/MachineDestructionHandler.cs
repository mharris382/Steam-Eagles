using Buildables;
using Buildings;
using Items;
using UnityEngine;
using Zenject;

namespace UI.Crafting.Destruction
{
    public class MachineDestructionHandler : DestructionHandler
    {
        public class Factory : PlaceholderFactory<Recipe, MachineDestructionHandler> { }
        public MachineDestructionHandler(Recipe recipe, DestructionPreview destructionPreview) : base(recipe, destructionPreview)
        {
        }

        public override bool HasDestructionTarget(Building building, BuildingCell cell)
        {
            var bMachines = building.GetComponent<BMachines>();
            return bMachines.Map.GetMachine(cell.cell2D) != null;
        }

        public override Vector2 GetDestructionTargetSize(Building building, BuildingCell cell)
        {
            var bMachines = building.GetComponent<BMachines>();
            var machine = bMachines.Map.GetMachine(cell.cell2D);
            if(machine == null)
                return Vector2.zero;
            return machine.MachineSize;
        }

        public override void Destruct(Building building, BuildingCell cell)
        {
            var bMachines = building.GetComponent<BMachines>();
            bMachines.Map.RemoveMachineAt(cell.cell2D);
        }
    }
}