using Buildings.Rooms.Tracking;
using UnityEngine;
using Zenject;

namespace Players.PCController.Interactions
{
    
    public class PCInteractionSystems : PCSystems<PCInteractionSystem>, IInitializable, ITickable, ILateTickable
    {
        private readonly ISystemFactory<PCInteractionSystem> _factory;

        public PCInteractionSystems(PCTracker pcTracker, PC.Factory pcFactory, ISystemFactory<PCInteractionSystem> factory) : base(pcTracker, pcFactory, factory)
        {
            Debug.Log("Created PCInteractionSystems");
            _factory = factory;
        }

        protected override PCInteractionSystem CreateSystemFor(PC pc) => _factory.Create(pc);

        public void Initialize()
        {
            Debug.Log("Initialized PCInteractionSystems");
        }

        public void Tick()
        {
            foreach (var pcInteractionSystem in GetAllSystems())
            {
                pcInteractionSystem.Tick();
            }
        }

        public void LateTick()
        {
            foreach (var pcInteractionSystem in GetAllSystems())
            {
                pcInteractionSystem.LateTick();
            }
        }
    }
}