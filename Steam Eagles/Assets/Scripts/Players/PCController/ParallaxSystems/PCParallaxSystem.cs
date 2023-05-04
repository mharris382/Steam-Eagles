using Players.PCController;
using Players.PCController.ParallaxSystems;
using UnityEngine;
using Zenject;

public class PCParallaxSystem : PCSystem, ITickable, ILateTickable
{
    private readonly ParallaxSprites _parallaxSprites;

    public PCParallaxSystem(PC pc, ParallaxSprites parallaxSprites) : base(pc)
    {
        _parallaxSprites = parallaxSprites;
        Debug.Log("Created PCParallaxSystem");
    }

    public void Tick()
    {
        Debug.Log("Ticking PCParallaxSystem");
    }

    public void LateTick()
    {
        Debug.Log("Late Ticking PCParallaxSystem");
    }
    
    public class Factory : PlaceholderFactory<PC, PCParallaxSystem>, ISystemFactory<PCParallaxSystem>
    {
    }

}