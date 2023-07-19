using System;

namespace Buildings
{
    public interface IPowerSupplier
    {
        IObservable<float> GetSupplyRateChanges();
        float GetSupplyRate();
        float Supply(float supply);
    }
    
    public interface IPowerConsumer
    {
        IObservable<float> GetConsumptionRateChanges();
        float GetConsumptionRate();
        void Consume(float amount);
    }
}