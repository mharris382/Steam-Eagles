using System;
using System.Collections;
using Buildables;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Buildings
{
    public class PowerSupplier : PowerTileUtility
    {
        [Serializable]
        public class Feedback
        {
            public float timeWithoutSupplyBeforeConsideredOffline = 5;
            public BoolReactiveProperty isOnline = new BoolReactiveProperty(true);
        }
        
        
        public float supplyRate = 1;
        public Feedback feedback;
        [ReadOnly]
        public float amountSupplied = 0;
        
        public UnityEvent<float> onPowerSupplied = new UnityEvent<float>();

        float _timeLastSupply = 0;

        
        
        public bool IsOnline => feedback.isOnline.Value;


        protected override void OnMachineBuilt(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            bool success = buildingPowerGrid.AddSupplier(tileSetterCell, () => supplyRate, t => {
                amountSupplied += t;
                onPowerSupplied.Invoke(t);
                return t;
            });
            if (!success) Debug.LogError($"{machineBase.name} Failed to add consumer to power grid at cell: {tileSetterCell}!");
        }

        protected override void OnMachineRemoved(BuildableMachineBase machineBase, BuildingCell tileSetterCell,
            BuildingPowerGrid buildingPowerGrid)
        {
            buildingPowerGrid.RemoveSupplier(tileSetterCell);
        }

        protected override void OnAwake()
        {
            onPowerSupplied.AsObservable().Where(t => t != 0).Subscribe(_ =>
            {
                _timeLastSupply = Time.time;
                StartCoroutine(StartOfflineTimer());
            }).AddTo(this);
        }

        IEnumerator StartOfflineTimer()
        {
            feedback.isOnline.Value = true;
            yield return new WaitForSeconds(feedback.timeWithoutSupplyBeforeConsideredOffline);
            yield return null;
            if(Time.time - _timeLastSupply > feedback.timeWithoutSupplyBeforeConsideredOffline) feedback.isOnline.Value = false;
            else feedback.isOnline.Value = true;
        }
    }
}