using Players.PCController;
using Players.PCController.ParallaxSystems;
using Zenject;

public class PCParallaxSystemsInstaller : Installer<PCParallaxSystemsInstaller>
{
    public override void InstallBindings()
    {
        Container.BindFactory<PC, PCParallaxSystem, PCParallaxSystem.Factory>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PCParallaxSystems>().AsSingle().NonLazy();
        Container.Bind<ParallaxSprites>().FromNew().AsSingle();
    }
}