using Buildings.Rooms.Tracking;
using CoreLib;
using FSM;
using Players.PCController;
using Zenject;

public class PCSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PCManager>().AsSingle().NonLazy();
        Container.BindFactory<int, PCTracker.TrackedPC, PC, PC.Factory>();
        Container.BindInterfacesAndSelfTo<PCTracker>().FromNew().AsSingle().NonLazy();
        
    }
    
}