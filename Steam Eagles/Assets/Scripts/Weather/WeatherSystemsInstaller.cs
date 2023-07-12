using System.Linq;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Weather.Storms;
using Zenject;

[InfoBox("Should be installed in scene context")]
public class WeatherSystemsInstaller : MonoInstaller
{
    public WeatherConfig config;
    
    public bool installDebug;
    public TestStorm testStorm;
    
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<WeatherRunner>().AsSingle().NonLazy();
        Container.Bind<WeatherConfig>().FromInstance(config).AsSingle().NonLazy();
        
        if (installDebug) Container.Bind<TestStorm>().FromInstance(testStorm).AsSingle().NonLazy();


        StormSystemsInstaller.Install(Container);
        Container.BindInterfacesTo<SlowTickUpdater>().AsSingle().NonLazy();
    }
    
}