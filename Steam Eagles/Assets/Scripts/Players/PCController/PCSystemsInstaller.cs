using Buildings.Rooms.Tracking;
using CoreLib;
using CoreLib.Interfaces;
using FSM;
using Players.PCController;
using UnityEngine;
using Zenject;

public class PCSystemsInstaller : MonoInstaller
{
    public bool enableDebugging = true;
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PCManager>().AsSingle().NonLazy();
        Container.BindFactory<int, PCTracker.TrackedPC, PC, PC.Factory>();
        Container.BindInterfacesAndSelfTo<PCTracker>().FromNew().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PCEventPublisher>().FromNew().AsSingle().NonLazy();
        Container.Bind<IPCViewFactory>().To<PCViewFactory>().AsSingle().NonLazy();
        Container.Bind<PlayerViewFactory>().AsSingle().NonLazy();
        if (enableDebugging)
        {
            Container.BindInterfacesAndSelfTo<PCEventDebugger>().AsSingle().NonLazy();
            Debug.Log("Enabled PC Event Debugger", this);
        }
        Container.BindInterfacesAndSelfTo<PCRegistry>().AsSingle().NonLazy();
    }

    
    
    public class PCEventDebugger : IInitializable
    {
        private readonly PCRegistry _pcRegistry;

        public PCEventDebugger(PCRegistry pcRegistry)
        {
            _pcRegistry = pcRegistry;
        }
        public void Initialize()
        {
            
        }
    }
}