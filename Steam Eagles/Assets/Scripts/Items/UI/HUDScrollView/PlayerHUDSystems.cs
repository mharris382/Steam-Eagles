using System;
using System.ComponentModel;
using Buildings.Rooms.Tracking;
using Players.PCController;
using Zenject;

namespace Items.UI.HUDScrollView
{
    public class PlayerHUDSystems : PCSystems<PlayerHUDSystem>, IInitializable, ITickable, ILateTickable
    {
        private readonly ISystemFactory<PlayerHUDSystem> _factory;

        public PlayerHUDSystems(PCTracker pcTracker, PC.Factory pcFactory, ISystemFactory<PlayerHUDSystem> factory) : base(pcTracker, pcFactory, factory)
        {
            _factory = factory;
        }

        public override PlayerHUDSystem CreateSystemFor(PC pc)
        {
            return _factory.Create(pc);
        }

        public void Initialize()
        {
            foreach (var playerHUDSystem in GetAllSystems())
            {
                playerHUDSystem.Initialize();
            }
        }

        public void Tick()
        {
            foreach (var playerHUDSystem in GetAllSystems())
            {
                playerHUDSystem.Tick();
            }
        }

        public void LateTick()
        {
            foreach (var playerHUDSystem in GetAllSystems())
            {
                playerHUDSystem.LateTick();
            }
        }
    }
    
    
}