using Buildings.Rooms.Tracking;
using Players.PCController;
using Players.PCController.ParallaxSystems;
using UnityEngine;
using Zenject;

public class PCParallaxSystems : PCSystems<PCParallaxSystem>, IInitializable, ITickable, ILateTickable
{
    private readonly ISystemFactory<PCParallaxSystem> _factory;
    private readonly ParallaxSprites _parallaxSprites;

    public override PCParallaxSystem CreateSystemFor(PC pc) => _factory.Create(pc);
    
    
    public void Tick()
    {
        Debug.Log("Ticking PCParallaxSystems");
        foreach (var pcParallaxSystem in GetAllSystems())
        {
            pcParallaxSystem.Tick();
        }
    }
    public void LateTick()
    {
        Debug.Log("Late Ticking PCParallaxSystems");
        foreach (var pcParallaxSystem in GetAllSystems())
        {
            pcParallaxSystem.LateTick();
        }
    }

    public PCParallaxSystems(PCTracker pcTracker, PC.Factory pcFactory, ISystemFactory<PCParallaxSystem> factory, ParallaxSprites parallaxSprites) : base(pcTracker, pcFactory, factory)
    {
        _factory = factory;
        _parallaxSprites = parallaxSprites;
        Debug.Log("Created PCParallaxSystems");
    }

    public void Initialize()
    {
        Debug.Log("Initialized PCParallaxSystems");
    }
}