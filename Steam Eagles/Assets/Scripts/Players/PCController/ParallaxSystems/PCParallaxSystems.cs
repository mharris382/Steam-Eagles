using Buildings.Rooms.Tracking;
using Players.PCController;
using Players.PCController.ParallaxSystems;
using UnityEngine;
using Zenject;

public class PCParallaxSystems : PCSystems<PCParallaxSystem>, IInitializable, ITickable, ILateTickable
{
    private PCParallaxSystem.Factory _factory;
    private ParallaxSprites _parallaxSprites;
    public PCParallaxSystems(PCTracker pcTracker, PC.Factory pcFactory,  PCParallaxSystem.Factory factory, ParallaxSprites parallaxSprites) : base(pcTracker, pcFactory, factory)
    {
        _factory = factory;
        _parallaxSprites = parallaxSprites;
        Debug.Assert(_factory != null, "Factory is null");
        Debug.Assert(_parallaxSprites != null, "ParallaxSprites is null");
        Debug.Log("Created PCParallaxSystems");
    }
    public override PCParallaxSystem CreateSystemFor(PC pc)
    {
        if (_factory == null || _parallaxSprites == null)
        {
            var resources = ParallaxResources.Resources;
            Debug.Assert(resources!=null);
            _factory = resources.Factory;
            _parallaxSprites = resources.Sprites;
        }
        Debug.Assert(_factory != null, "Factory is null");
        Debug.Assert(pc != null, "PC is null");
        if (_factory == null)
        {
            Debug.LogError("Factory is null");
            var pcParallaxSystem = new PCParallaxSystem(pc, _parallaxSprites);
            return pcParallaxSystem;
        }
        return _factory.Create(pc);
    }


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

    public override void Dispose()
    {
        base.Dispose();
    }

 

    public void Initialize()
    {
        Debug.Log("Initialized PCParallaxSystems");
    }
}