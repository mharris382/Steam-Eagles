using Buildings.Rooms.Tracking;
using UnityEngine;
using Zenject;

namespace Players.PCController.Tools
{
    public class PCToolSystemsInstaller : Installer<PCToolSystemsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactoryCustomInterface<PC, PCToolSystem, PCToolSystem.Factory, ISystemFactory<PCToolSystem>>();
            Container.BindInterfacesTo<PCToolSystems>().AsSingle().NonLazy();
        }
    }
    public class PCToolSystems : PCSystems<PCToolSystem>, ITickable
    {
        private readonly ISystemFactory<PCToolSystem> _factory;

        public PCToolSystems(PCTracker pcTracker, PC.Factory pcFactory, ISystemFactory<PCToolSystem> factory) : base(pcTracker, pcFactory, factory)
        {
            _factory = factory;
        }

        public override PCToolSystem CreateSystemFor(PC pc)
        {
            return _factory.Create(pc);
        }

        public void Tick()
        {
            Debug.Log("Ticking PCToolSystems");
            foreach (var pcToolSystem in GetAllSystems())
            {
                pcToolSystem.Tick();
            }
        }
    }
}