using CoreLib.Pickups.PickupSpawner;
using Zenject;

namespace CoreLib.Pickups
{
    public class PickupInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PickupDropper>().AsSingle().NonLazy();
            Container.Bind<PickupCache>().AsSingle().NonLazy();
        }
    }
}