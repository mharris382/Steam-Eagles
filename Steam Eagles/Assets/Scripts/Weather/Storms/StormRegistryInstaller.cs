using Zenject;

namespace Weather.Storms
{
    public class StormRegistryInstaller : Installer<StormRegistryInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<StormRegistry>().AsSingle().NonLazy();
            Container.BindFactory<IStormSubject, StormSubject, StormSubject.Factory>().AsSingle();
            Container.Bind<StormSubjectsRegistry>().AsSingle().NonLazy();
        }
    }
}