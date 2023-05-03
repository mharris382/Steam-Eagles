using CoreLib;
using Players.PCController;
using UnityEngine;
using Zenject;
using TrackedPC = Buildings.Rooms.Tracking.PCTracker.PC;

public class PCSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PCManager>().AsSingle().NonLazy();
        Container.BindFactory<int, TrackedPC, PC, PC.Factory>();
        Container.BindInterfacesAndSelfTo<TestMe>().FromNew().AsSingle().NonLazy();
    }

    class TestMe : IInitializable
    {
        private readonly PCManager _pcManager;

        public TestMe(PCManager pcManager)
        {
            Debug.Log("Created TestMe from PCSystemsInstaller");
            _pcManager = pcManager;
        }
        public void Initialize()
        {
            Debug.Log("Initialize TestMe");
            Debug.Log(_pcManager);
        }
    }

    
}