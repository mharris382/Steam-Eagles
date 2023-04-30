using System;
using Buildings;
using UniRx;
using UnityEngine;

namespace Power.Steam
{
    public abstract class SteamNode : NetworkNode, INetworkUpdatable
    {
        private float _pressure;
        private Subject<Unit> _updateSubject = new Subject<Unit>();

        public override BuildingLayers Layer => BuildingLayers.PIPE;
        public abstract float Capacity { get; }

        public float AvailableSpace => Capacity - Pressure;

        public float Pressure
        {
            get => _pressure;
            set => _pressure = Mathf.Clamp(value, 0, Capacity);
        }

        public float InFlow { get; set; }
        public float OutFlow { get; set; }

        public IObservable<Unit> OnNodeUpdate => _updateSubject;

        protected SteamNode(Vector3Int cell) : base(cell) { }


        public virtual void UpdateNetwork()
        {
            Pressure += InFlow;
            Pressure -= OutFlow;
            Pressure = Mathf.Clamp(Pressure, 0, Capacity);
            _updateSubject.OnNext(Unit.Default);
        }
    }
    
    public class PipeNode : SteamNode
    {
        public PipeNode(Vector3Int cell) : base(cell) { }
        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.pipeCapacity;
    }

    public class SupplierNode : SteamNode, INetworkSupplier
    {
        private Subject<float> _onSupplied = new Subject<float>();
        public IObservable<float> OnSupplied => _onSupplied;
        
        public float AmountSuppliedPerUpdate { get; set; }
        public SupplierNode(Vector3Int cell, float amountSuppliedPerUpdate = 1) : base(cell)
        {
            AmountSuppliedPerUpdate = amountSuppliedPerUpdate;
        }

        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.maxPressure;
        public float ProductionTarget => AmountSuppliedPerUpdate;

        public float TryGetSupply(float amount)
        {
            if (Pressure < amount)
            {
                amount = Pressure;
            }
            Pressure -= amount;
            return amount;
        }

        public override void UpdateNetwork()
        {
            base.UpdateNetwork();

            if (TryAddPressureToNode(out var amount)) 
                _onSupplied.OnNext(amount);
        }

        private bool TryAddPressureToNode(out float amount)
        {
            
            var prevPressure = Pressure;
            Pressure += AmountSuppliedPerUpdate;
            Pressure = Mathf.Clamp(Pressure, 0, Capacity);
            amount = Pressure - prevPressure;
            return amount > 0;
        }
    }
    public class ConsumerNode : SteamNode, INetworkConsumer
    {
        private Subject<float> _onConsumed = new Subject<float>();
        public float MaxConsumptionPerUpdate { get; set; }
        public IObservable<float> OnConsumed => _onConsumed;
        public ConsumerNode(Vector3Int cell, float maxConsumptionPerUpdate = float.MaxValue) : base(cell) => MaxConsumptionPerUpdate = maxConsumptionPerUpdate;

        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.maxPressure;
        
        public float ConsumptionTarget => MaxConsumptionPerUpdate;

        public float TryConsume(float amount)
        {
            if (Pressure + amount > MaxConsumptionPerUpdate) amount = MaxConsumptionPerUpdate - Pressure;
            Pressure += amount;
            return amount;
        }

        public override void UpdateNetwork()
        {
            base.UpdateNetwork();

            if (TryRemovePressureFromNode(out var amt)) 
                _onConsumed.OnNext(amt);
        }

        private bool TryRemovePressureFromNode(out float amount)
        {
            var prevPressure = Pressure;
            Pressure -= MaxConsumptionPerUpdate;
            Pressure = Mathf.Clamp(Pressure, 0, Capacity);
            amount = prevPressure - Pressure;
            return amount > 0;
        }
    }
}