using System;

namespace Buildings
{
    [Serializable]
    public class HyperGenerator
    {
           
        public PowerStorageUnit internalPowerStorageUnit;
        public OverridablePowerSupplier[] outputPipes;
        public OverridablePowerConsumer[] inputPipes;


        

        public void Initialize()
        {
            foreach (var overridablePowerSupplier in outputPipes)
            {
                overridablePowerSupplier.SetOverride(GetProductionRate, Produce);
            }
            foreach (var overridablePowerConsumer in inputPipes)
            {
                overridablePowerConsumer.SetOverride(GetConsumptionRate, Consume);
            }
        }

        public float StoredAmount
        {
            get => internalPowerStorageUnit.currentStored;
            set => internalPowerStorageUnit.currentStored = value;
        }

        public float Capacity => internalPowerStorageUnit.capacity;
        
        public float GetConsumptionRate() => internalPowerStorageUnit.MaxCanAddRaw;

        void Consume(float amount) => internalPowerStorageUnit.currentStored+=amount;

        public  float GetProductionRate() => internalPowerStorageUnit.MaxCanRemoveRaw;

        float Produce(float amount)
        {
            internalPowerStorageUnit.currentStored-=amount;
            return amount;
        }

        public void Tick(float deltaTime)
        {
            
        }

        public void Dispose()
        {
            internalPowerStorageUnit.Dispose();
        }
    }
}