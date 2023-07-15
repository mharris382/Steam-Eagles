using System;
using Buildables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Buildings
{
    public class PowerConsumer : PowerTileUtility
    {
        public float consumptionRate = 1;
        [ReadOnly]
        public float amountConsumed = 0;
        
        public UnityEvent<float> onPowerConsumed = new UnityEvent<float>();

        protected override void OnMachineBuilt(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            bool success = buildingPowerGrid.AddConsumer(tileSetterCell, () => consumptionRate, t =>
            {
                amountConsumed += 1;
                onPowerConsumed.Invoke(t);
            });
            if (!success) Debug.LogError($"{machineBase.name} Failed to add consumer to power grid at cell: {tileSetterCell}!");
        }

        protected override void OnMachineRemoved(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            buildingPowerGrid.RemoveConsumer(tileSetterCell);
        }
    }
}