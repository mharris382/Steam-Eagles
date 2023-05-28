using Buildings.Rooms;
using Zenject;

namespace Buildings.Damage
{
    public class BuildingDamageInstaller : Installer<BuildingDamageInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<BuildingDamageResources>().AsSingle().NonLazy();
            
        }
    }
}