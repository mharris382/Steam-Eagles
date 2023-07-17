using System;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables
{
   
    public class HyperPump : Machine<HyperPump>
    {
        [Range(1, 5)] [SerializeField] private int pressesToFillStorage = 1;
        [Range(0.5f, 3)] [SerializeField] private float productionModifier = 1;
        [SerializeField] private PowerStorageUnit internalPumpStorage  = new PowerStorageUnit() {
            capacity = 2,
            outflowRate = 100,
            inflowRate = 100,
        };
        public OverridablePowerSupplier steamOutputPipe;
        
        
        private BuildableMachine _buildableMachine;

        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        public Building Building => BuildableMachine.Building;
        public Vector2Int GetOutputCell()
        {
            return default;
        }

        [ShowInInspector, BoxGroup("Debugging"), ReadOnly,HideInEditorMode]
        public bool IsProducing
        {
            get => AmountStored > 0;
        }
        
        [ShowInInspector, BoxGroup("Debugging"), ReadOnly,HideInEditorMode]
        public float ProductionRate
        {
            get => internalPumpStorage.MaxCanRemoveRaw * productionModifier;
        }

        [ShowInInspector, BoxGroup("Debugging"), ReadOnly, ProgressBar(0, nameof(StorageCapacity)),HideInEditorMode]
        public float AmountStored
        {
            get => internalPumpStorage.currentStored;
            set => internalPumpStorage.currentStored = value;
        }


        private float StorageCapacity => internalPumpStorage.capacity;

        private void Start()
        {
            float GetProductionRate() => ProductionRate;
            float TakeAmount(float amount)
            {
                internalPumpStorage.currentStored -= (amount/productionModifier);
                return amount;
            }
            steamOutputPipe.SetOverride(GetProductionRate, TakeAmount);
        }
        
        public void Interact()
        {
            float amount = StorageCapacity / pressesToFillStorage;
            internalPumpStorage.currentStored += amount;
        }
    }
}