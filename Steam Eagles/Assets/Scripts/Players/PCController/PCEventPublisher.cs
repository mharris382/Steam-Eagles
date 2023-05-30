using Buildings.Rooms.Tracking;
using CoreLib.Signals;
using JetBrains.Annotations;
using Players.PCController;
using UniRx;
using Zenject;

[UsedImplicitly]
public class PCEventPublisher : IInitializable
{
    private readonly PCManager _pcManager;
    private readonly PCTracker _pcTracker;
    private readonly PCRegistry _pcRegistry;


    public PCEventPublisher(PCManager pcManager, PCTracker pcTracker, PCRegistry pcRegistry)
    {
        _pcManager = pcManager;
        _pcTracker = pcTracker;
        _pcRegistry = pcRegistry;
    }
        
    public void Initialize()
    {
        _pcTracker.OnPCChangedOrExists
            .Subscribe(t => MessageBroker.Default.Publish(new PCInfo(t.Item1, t.Item2.Instance, t.Item2)));
    }
}