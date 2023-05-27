using Sirenix.OdinInspector;
using Zenject;

namespace Buildings.DI
{
    public class BuildingGameObjectInstaller : MonoInstaller
    {
        [ToggleGroup(nameof(overrideTileAssets))]
        public bool overrideTileAssets;
        [InlineProperty, ToggleGroup(nameof(overrideTileAssets))]
        public TileAssets tileAssets;
        public override void InstallBindings()
        {
            Container.Bind<Building>().FromInstance(GetComponentInChildren<Building>()).AsSingle();
            Container.BindFactory<Building, BuildingMap, BuildingMap.Factory>().AsSingle().NonLazy();
            
            if (overrideTileAssets)
            {
                Container.Rebind<TileAssets>().FromInstance(tileAssets).AsSingle().NonLazy();
            }
        }
    }
}