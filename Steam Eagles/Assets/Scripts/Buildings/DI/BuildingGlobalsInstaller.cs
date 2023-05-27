using System;
using Buildings;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using CoreLib;
using Utilities.AddressablesUtils;


public class BuildingGlobalsInstaller : MonoInstaller
{
    public GlobalBuildingConfig config;
    [HideLabel]
    public TileAssets tileAssets;
    public override void InstallBindings()
    {
        Container.Bind<GlobalBuildingConfig>().FromInstance(config).AsSingle().NonLazy();
        //Container.Bind<MachineCellMap>().AsSingle().NonLazy();
        Container.Bind<BuildingRegistry>().AsSingle().NonLazy();
        Container.Bind<TileAssets>().FromInstance(tileAssets).AsSingle().NonLazy();
    }
}

[InlineProperty, HideLabel]
public class GlobalBuildingConfig : ConfigBase
{
    
}



