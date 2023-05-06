using Players.PCController.ParallaxSystems;
using Zenject;

namespace Players.PCController.Interactions
{
    public class PCInteractionSystemsInstaller : Installer<PCInteractionSystemsInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindFactoryCustomInterface<PC, PCInteractionSystem, PCInteractionSystem.Factory,
                    ISystemFactory<PCInteractionSystem>>();
            Container.BindInterfacesAndSelfTo<PCInteractionSystems>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InteractionCameras>().AsSingle().NonLazy();
            
            Container.Bind<IPCInteractionStateListener>().To<TestPCInteractionStateListener>().AsTransient().NonLazy();
            Container.Bind<IPCInteractionStateListener>().To<TestPCInteractionStateListener2>().AsTransient().NonLazy();
            Container.Bind<IPCInteractionStateListener>().To<DisableToolsDuringInteraction>().AsTransient().NonLazy();
        }
    }



   
}