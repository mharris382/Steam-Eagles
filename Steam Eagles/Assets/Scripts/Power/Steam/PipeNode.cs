using Buildings;
using UnityEngine;

namespace Power.Steam
{
    public abstract class SteamNode : NetworkNode
    {
        public override BuildingLayers Layer => BuildingLayers.PIPE;
        
        protected SteamNode(Vector3Int cell) : base(cell)
        {
        }

        public abstract float Capacity { get; }

        private float _pressure;
        public float Pressure
        {
            get => _pressure;
            set => _pressure = Mathf.Clamp(value, 0, Capacity);
        }
        public float Flow { get; }
        
        
        public float AvailableSpace => Capacity - Pressure;
        
        
    }
    
    public class PipeNode : SteamNode
    {
        public PipeNode(Vector3Int cell) : base(cell) { }
        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.pipeCapacity;
    }

    public class SupplierNode : SteamNode, INetworkSupplier
    {
        public float AmountSuppliedPerUpdate { get; set; }
        public SupplierNode(Vector3Int cell, float amountSuppliedPerUpdate = 1) : base(cell)
        {
            AmountSuppliedPerUpdate = amountSuppliedPerUpdate;
        }

        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.maxPressure;
        public float TryGetSupply(float amount)
        {
            if (Pressure < amount)
            {
                amount = Pressure;
            }
            Pressure -= amount;
            return amount;
        }
        public void UpdateNetwork() => Pressure = AmountSuppliedPerUpdate;
    }
    public class ConsumerNode : SteamNode, INetworkConsumer
    {
        public float MaxConsumptionPerUpdate { get; set; }
        public ConsumerNode(Vector3Int cell, float maxConsumptionPerUpdate = float.MaxValue) : base(cell)
        {
            MaxConsumptionPerUpdate = maxConsumptionPerUpdate;
        }

        public override float Capacity => PowerNetworkConfigs.Instance.steamNetworkConfig.maxPressure;
        public void UpdateNetwork() => Pressure = 0;
        public float TryConsume(float amount)
        {
            if (Pressure + amount > MaxConsumptionPerUpdate) amount = MaxConsumptionPerUpdate - Pressure;
            Pressure += amount;
            return amount;
        }
    }
}