using Buildings;
using Zenject;

public class BuildingGlobalsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        //Container.Bind<MachineCellMap>().AsSingle().NonLazy();
    }
}