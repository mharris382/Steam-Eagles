using CoreLib;
using FSM;
using Players.PCController;
using Players.PCController.Interactions;
using Zenject;
using TrackedPC = Buildings.Rooms.Tracking.PCTracker.PC;

public class PCSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PCManager>().AsSingle().NonLazy();
        Container.BindFactory<int, TrackedPC, PC, PC.Factory>();
        PCParallaxSystemsInstaller.Install(Container);
        PCInteractionSystemsInstaller.Install(Container);
    }
    
}