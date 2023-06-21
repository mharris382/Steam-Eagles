using Buildings.CoreData;
using Buildings.Damage;
using Buildings.Rooms;
using JetBrains.Annotations;
using SaveLoad;
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
            BuildingDamageInstaller.Install(Container);
            Container.Bind<Building>().FromInstance(GetComponentInChildren<Building>()).AsSingle();
            
            // Container.Bind<EntityInitializer>().FromInstance(GetComponentInChildren<EntityInitializer>()).AsSingle();
            if (overrideTileAssets)
            {
                Container.Rebind<TileAssets>().FromInstance(tileAssets).AsSingle().NonLazy();
            }

            Container.BindInterfacesAndSelfTo<BuildingCoreData>().AsSingle().NonLazy();
            TilemapSaveDataV3Installer.Install(Container);
            
            
            Container.BindFactory<BuildingLayers, IRoomTilemapTextureSaveLoader, TexSaveLoadFactory>().FromFactory<TexSaveLoadFactoryImpl>();
            Container.BindFactory<Room, BuildingLayers, RoomTilemapTextures.RoomTexture, RoomTilemapTextures.RoomTexture.Factory>().AsSingle().NonLazy();
            ReflectedInstaller<IRoomTilemapTextureSaveLoader>.Install(Container, ContainerLevel.GAMEOBJECT);
        }
    }

    [UsedImplicitly]
    [ContainerLevel(ContainerLevel.GAMEOBJECT)]
    public class SolidSaveLoader : NullLayerSaveLoader
    {
        public override BuildingLayers TargetLayer => BuildingLayers.SOLID;
    }
    
    [ContainerLevel(ContainerLevel.GAMEOBJECT)]
    [UsedImplicitly]public class WallSaveLoader : NullLayerSaveLoader
    {
        public override BuildingLayers TargetLayer => BuildingLayers.WALL;
    }
    
    [ContainerLevel(ContainerLevel.GAMEOBJECT)]
    [UsedImplicitly]public class LadderSaveLoader : NullLayerSaveLoader
    {
        public override BuildingLayers TargetLayer => BuildingLayers.LADDERS;
    }
    
    [ContainerLevel(ContainerLevel.GAMEOBJECT)]
    [UsedImplicitly]public class WireSaveLoader : NullLayerSaveLoader
    {
        public override BuildingLayers TargetLayer => BuildingLayers.WIRES;
    }
    
    [ContainerLevel(ContainerLevel.GAMEOBJECT)]
    [UsedImplicitly]public class PlatformSaveLoader : NullLayerSaveLoader
    {
        public override BuildingLayers TargetLayer => BuildingLayers.PLATFORM;
    }
}