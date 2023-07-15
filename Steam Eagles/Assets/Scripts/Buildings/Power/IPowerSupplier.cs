namespace Buildings
{
    public interface IPowerSupplier
    {
        float GetSupplyRate();
        float Supply(float supply);
    }
    
    public interface IPowerConsumer
    {
        float GetConsumptionRate();
        void Consume(float amount);
    }
}