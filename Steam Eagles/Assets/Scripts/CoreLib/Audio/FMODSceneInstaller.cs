using Zenject;

using FMODEvent = FMODUnity.EventReference;

public class FMODSceneInstaller : MonoInstaller
{
    public FMODEvent musicEvent;
    public FMODEvent ambianceEvent;
    public override void InstallBindings()
    {
        Container.Bind<FMODSceneLoadCallback>().FromComponentOn(gameObject).AsSingle().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.MUSIC).FromInstance(musicEvent).AsCached().WhenInjectedInto<FMODMusicPlayer>().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.AMBIANCE).FromInstance(ambianceEvent).AsCached().NonLazy();
        Container.BindInterfacesAndSelfTo<FMODMusicPlayer>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FMODAmbiancePlayer>().AsSingle().NonLazy();
    }
}

public static class FMODEventIDs
{
    public const string MUSIC = "music";
    public const string AMBIANCE = "ambiance";
}