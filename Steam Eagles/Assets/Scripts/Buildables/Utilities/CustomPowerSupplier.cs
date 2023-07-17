using Buildables;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;

namespace Buildings
{
    public abstract class CustomPowerSupplier : PowerTileUtility
    {
        [BoxGroup("Debugging"), SerializeField, ReadOnly, HideInEditorMode] private FloatReactiveProperty supplyRate = new FloatReactiveProperty();
        [BoxGroup("Debugging"), SerializeField, ReadOnly, HideInEditorMode] private BoolReactiveProperty isActive = new();
        private PowerTimerUtility _powerTimer;

        public UnityEvent<bool> onActiveChanged;

        protected override void OnAwake()
        {
            isActive.Subscribe(t => onActiveChanged.Invoke(t)).AddTo(this);
        }
        [Inject] void Install(PowerTimerUtility.Factory powerTimerUtility)
        {
            _powerTimer = powerTimerUtility.Create();
            _powerTimer.IsOnline.Subscribe(t => isActive.Value = t).AddTo(this);
        }
        
        protected override void OnMachineBuilt(BuildableMachineBase machineBase, BuildingCell tileSetterCell, BuildingPowerGrid buildingPowerGrid)
        {
            buildingPowerGrid.AddSupplier(tileSetterCell, _GetSupplyRate, _RemoveSupply);
        }

        protected override void OnMachineRemoved(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            buildingPowerGrid.RemoveSupplier(tileSetterCell);
        }

        private float _RemoveSupply(float amount)
        {
            var result = RemoveSupply(amount);
            _powerTimer.OnPowerSupplied(amount);
            return result;
        }

        private float _GetSupplyRate()
        {
            var result = GetSupplyRate();
            supplyRate.Value = result;
            return result;
        }
        
        protected abstract float GetSupplyRate();

        protected abstract float RemoveSupply(float amount);
    }
}