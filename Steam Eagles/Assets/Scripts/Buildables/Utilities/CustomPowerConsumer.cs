using Buildables;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;

namespace Buildings
{
    public abstract class CustomPowerConsumer : PowerTileUtility
    {
        
        private PowerTimerUtility _powerTimer;
        [FormerlySerializedAs("supplyRate")] [BoxGroup("Debugging"), SerializeField, ReadOnly, HideInEditorMode] private FloatReactiveProperty consumptionRate = new FloatReactiveProperty();
        [BoxGroup("Debugging"), SerializeField, ReadOnly, HideInEditorMode] private BoolReactiveProperty isActive = new();
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
            buildingPowerGrid.AddConsumer(tileSetterCell, _GetConsumptionRate, _Consume);
        }

        protected override void OnMachineRemoved(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            buildingPowerGrid.RemoveSupplier(tileSetterCell);
        }

        protected abstract float GetConsumptionRate();
        protected abstract void Consume(float amount);
        
        
        private void _Consume(float amount)
        {
            Consume(amount);
            _powerTimer.OnPowerSupplied(amount);
        }

        private float _GetConsumptionRate()
        {
            var result = GetConsumptionRate();
            consumptionRate.Value = result;
            return result;
        }

    }
}