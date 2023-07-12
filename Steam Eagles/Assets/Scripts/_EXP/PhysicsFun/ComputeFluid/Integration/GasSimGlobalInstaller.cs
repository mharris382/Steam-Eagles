using System;
using Buildings;
using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class GasSimGlobalInstaller : MonoInstaller
{
    [ShowInInspector]
    public SimConfig simConfig
    {
        get => GasSimConfig.Instance.defaultSimConfig;
        set => GasSimConfig.Instance.defaultSimConfig = value;
    }

    public override void InstallBindings()
    {
        Container.Bind<SimConfig>().FromMethod(ResolveSimConfig).AsSingle();
    }

    SimConfig ResolveSimConfig(InjectContext context) => GasSimConfig.Instance.defaultSimConfig;
}