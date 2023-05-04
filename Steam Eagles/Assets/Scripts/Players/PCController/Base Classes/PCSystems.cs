using Buildings.Rooms.Tracking;
using Players.PCController;

/// <summary>
/// this version requires a factory to be passed in, which is a bit more verbose, but allows for more flexibility
/// <see cref="ISystemFactory{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PCSystems<T> : PCSystemsBase<T> where T : PCSystem
{
    private readonly ISystemFactory<T> _factory;
    protected PCSystems(PCTracker pcTracker, PC.Factory pcFactory, ISystemFactory<T> factory) : base(pcTracker, pcFactory)
    {
        _factory = factory;
    }
}