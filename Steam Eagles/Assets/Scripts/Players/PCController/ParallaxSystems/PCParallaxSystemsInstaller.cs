using Players.PCController;
using Players.PCController.ParallaxSystems;
using Zenject;

public class PCParallaxSystemsInstaller : Installer<PCParallaxSystemsInstaller>
{
    public override void InstallBindings()
    {
        Container.BindFactoryCustomInterface<PC, PCParallaxSystem, PCParallaxSystem.Factory, ISystemFactory<PCParallaxSystem>>();
        Container.BindInterfacesAndSelfTo<PCParallaxSystems>().AsSingle().NonLazy();
        Container.Bind<ParallaxSprites>().FromNew().AsSingle();
    }
}