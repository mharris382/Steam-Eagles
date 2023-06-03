namespace Power
{
    public interface ISteamConsumer
    {
        bool IsActive { get; }
        float GetSteamConsumptionRate();
        void ConsumeSteam(float amount);
    }

    public interface ISteamProducer
    {
        bool IsActive { get; }
        float GetSteamProductionRate();
        void ProduceSteam(float amount);
    }
}