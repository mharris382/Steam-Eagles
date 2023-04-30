namespace Power
{
    public interface INetworkSupplier : INetworkUpdatable
    {
        float ProductionTarget { get; }
        float TryGetSupply(float amount);
    }
}