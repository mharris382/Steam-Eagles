using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

[InfoBox("Should be installed in scene context")]
public class WeatherSystemsInstaller : MonoInstaller
{
    public WeatherConfig config;
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<WeatherRunner>().AsSingle().NonLazy();
        Container.Bind<WeatherConfig>().FromInstance(config).AsSingle().NonLazy();
    }
    
}