public interface ISimulationStage
{
    void BeginStage();
    bool IsCompleted { get; }
}