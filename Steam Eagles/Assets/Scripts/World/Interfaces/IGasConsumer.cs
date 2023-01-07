public interface IGasConsumer
{
    bool enabled { get; }
    int GetRequestedSupply();
    void ReceiveSupply(int amount);
}