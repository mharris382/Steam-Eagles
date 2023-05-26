using Zenject;

namespace Buildings.DI
{
    public class BuildingGameObjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Building>().FromInstance(GetComponentInChildren<Building>()).AsSingle();
            Container.BindFactory<Building, BuildingMap, BuildingMap.Factory>().AsSingle().NonLazy();
        }
    }
}