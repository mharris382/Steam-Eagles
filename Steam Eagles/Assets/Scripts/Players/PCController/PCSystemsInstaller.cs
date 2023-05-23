using Buildings.Rooms.Tracking;
using CoreLib;
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

        if (enableDebugging)
        {
            Container.BindInterfacesAndSelfTo<PCEventDebugger>().AsSingle().NonLazy();
            Debug.Log("Enabled PC Event Debugger", this);
        }
        Container.Bind<GlobalPCInfo>().AsSingle().NonLazy();
    }

    
    
    public class PCEventDebugger : IInitializable
    {
        private readonly GlobalPCInfo _pcInfo;

        public PCEventDebugger(GlobalPCInfo pcInfo)
        {
            _pcInfo = pcInfo;
        }
        public void Initialize()
        {
            
        }
    }
}