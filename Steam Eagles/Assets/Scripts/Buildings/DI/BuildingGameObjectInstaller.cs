using System;
using Buildings.CoreData;
using Buildings.Damage;
using Buildings.Graph;
using Buildings.Rooms;
using JetBrains.Annotations;
using SaveLoad;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.DI
{
  
    public class BuildingGameObjectInstaller : MonoInstaller
    {
        [ToggleGroup(nameof(overrideTileAssets))]
        public bool overrideTileAssets;
        [InlineProperty, ToggleGroup(nameof(overrideTileAssets))]
        public TileAssets tileAssets;
        public PowerConfig powerConfig;
        
        [Required]
        public ElectricityLineHandler electricityLineHandlerPrefab;
        
        public override void InstallBindings()
        {
            BuildingDamageInstaller.Install(Container);
            var building = GetComponentInChildren<Building>();
            if (building == null) building = GetComponentInParent<Building>();
            Debug.Assert(building != null, "building != null", this);
            Container.Bind<Building>().FromInstance(building).AsCached().IfNotBound();
            
            // Container.Bind<EntityInitializer>().FromInstance(GetComponentInChildren<EntityInitializer>()).AsSingle();
            if (overrideTileAssets)
            {
                Container.Rebind<TileAssets>().FromInstance(tileAssets).AsSingle().NonLazy();
            }

            Container.BindInterfacesAndSelfTo<BuildingCoreData>().AsCached().NonLazy();

            Container.Bind<PowerConfig>().FromInstance(powerConfig).AsSingle().NonLazy();

            Container.BindFactory<BuildingLayers, IRoomTilemapTextureSaveLoader, TexSaveLoadFactory>().FromFactory<TexSaveLoadFactoryImpl>();
            Container.Bind<TilemapsSaveDataV3>().AsSingle().NonLazy();
            Container.BindFactory<Room, RoomTilemapTextures, RoomTilemapTextures.Factory>().AsSingle().NonLazy();
            Container.BindFactory<Room, BuildingLayers, RoomTilemapTextures.RoomTexture, RoomTilemapTextures.RoomTexture.Factory>().AsSingle().NonLazy();
            ReflectedInstaller<IRoomTilemapTextureSaveLoader>.Install(Container, ContainerLevel.GAMEOBJECT);


            Container.BindInterfacesAndSelfTo<PipeTilemapGraph>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WireTilemapGraph>().AsSingle().NonLazy();
            
            Container.Bind<BuildingPowerGrid>()
                .FromMethod(context => context.Container.Resolve<Building>().Map.PowerGrid).AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ElectricityConsumers>().AsSingle().NonLazy();
            Container
                .BindFactory<IElectricityConsumer, ElectricityConsumers.ElectricityConsumerWrapper,
                    ElectricityConsumers.ElectricityConsumerWrapper.Factory>().AsSingle().NonLazy();

            Debug.Assert(electricityLineHandlerPrefab != null, $"Missing electricityLineHandlerPrefab on {name}",this);
            Container.Bind<ElectricityLineHandler>().FromComponentInNewPrefab(electricityLineHandlerPrefab).UnderTransform(transform).AsSingle()
                .NonLazy();
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