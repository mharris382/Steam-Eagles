using Buildables;
using Buildings;
using Items;
using UI.Crafting.Events;
using UnityEngine;
using Zenject;

namespace UI.Crafting.Destruction
{
    public class MachineDestructionHandler : DestructionHandler
    {
        private readonly CraftingEventPublisher _eventPublisher;
        private BuildableMachineBase _destructTarget;

        public class Factory : PlaceholderFactory<Recipe, MachineDestructionHandler> { }
        public MachineDestructionHandler(Recipe recipe, DestructionPreview destructionPreview, CraftingEventPublisher eventPublisher) : base(recipe, destructionPreview)
        {
            _eventPublisher = eventPublisher;
        }

        public override bool HasDestructionTarget(Building building, BuildingCell cell)
        {
            var bMachines = building.GetComponent<BMachines>();
            _destructTarget = bMachines.Map.GetMachine(cell.cell2D);
            return _destructTarget != null;
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
            var machine = bMachines.Map.GetMachine(cell.cell2D);
            if (machine != null)
            {
                _eventPublisher.OnPrefabDeconstruct(cell, machine.gameObject, machine.IsFlipped);
                bMachines.Map.RemoveMachineAt(cell.cell2D);    
            }
        }
    }
}