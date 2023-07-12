using Buildings.Rooms.Tracking;
using Players.PCController.Interactions;
using Players.PCController.ParallaxSystems;
using Players.PCController.RoomCamera;
using Players.PCController.Tools;
using Zenject;

public class PCSceneSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // PCParallaxSystemsInstaller.Install(Container);
        Container.BindFactory<Players.PCController.PC, PCParallaxSystem, PCParallaxSystem.Factory>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PCParallaxSystems>().AsSingle().NonLazy();
        Container.Bind<ParallaxSprites>().FromNew().AsSingle();
        PCInteractionSystemsInstaller.Install(Container);
        //PCRoomCameraInstaller.Install(Container);
        PCToolSystemsInstaller.Install(Container);
        Container.Bind<ParallaxResources>().AsSingle().NonLazy();
    }
}

public class ParallaxResources
{
    private readonly PCParallaxSystem.Factory _factory;
    private readonly ParallaxSprites _sprites;
    private static ParallaxResources _resources;

    public ParallaxSprites Sprites => _sprites;
    public PCParallaxSystem.Factory Factory => _factory;
    public static ParallaxResources Resources => _resources;
    public ParallaxResources(PCParallaxSystem.Factory factory, ParallaxSprites sprites)
    {
        _factory = factory;
        _sprites = sprites;
        _resources = this;
    }
}