using CoreLib;
using FSM;
using Players.PCController;
using Zenject;
using TrackedPC = Buildings.Rooms.Tracking.PCTracker.PC;

public class PCSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PCManager>().AsSingle().NonLazy();
        Container.BindFactory<int, TrackedPC, PC, PC.Factory>();
        PCParallaxSystemsInstaller.Install(Container);
    }
    
}