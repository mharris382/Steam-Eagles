namespace Power
{
    public interface INetworkConsumer : INetworkUpdatable
    {
        float ConsumptionTarget { get; }
        float TryConsume(float amount);
    }
}