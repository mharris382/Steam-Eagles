using Buildables;
using Buildables.Utilities;
using UnityEngine;
using UniRx;

namespace Buildings
{
    [RequireComponent(typeof(TileSetter))]
    public abstract class PowerTileUtility : MonoBehaviour
    {
        private TileSetter _tileSetter;
        public TileSetter TileSetter => _tileSetter ? _tileSetter : _tileSetter = GetComponent<TileSetter>();

        private void Awake()
        {
            TileSetter.OnMachineBaseBuilt.Subscribe(OnMachineBuilt).AddTo(this);
            TileSetter.OnMachineBaseRemoved.Subscribe(OnMachineRemoved).AddTo(this);
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        protected void OnMachineBuilt(BuildableMachineBase machineBase) => OnMachineBuilt(machineBase, TileSetter.Cell, machineBase.Building.Map.PowerGrid);
        protected void OnMachineRemoved(BuildableMachineBase machineBase) => OnMachineRemoved(machineBase, TileSetter.Cell, machineBase.Building.Map.PowerGrid);


        protected abstract void OnMachineBuilt(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid);
        protected abstract void OnMachineRemoved(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid);
    }
}