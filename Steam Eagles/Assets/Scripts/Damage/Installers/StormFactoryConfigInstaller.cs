using System;
using System.Linq;
using Damage.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

[InfoBox("Should be bound at Project Context level i think. currently it is responsible for binding the factory that creates the active storms. " +
         "weather is being refactored so now this is dependent on the weather systems, so this needs to be bound below the weather systems container.  " +
         "Currently i see no reason it cannot still be bound at the project context level")]
[CreateAssetMenu(menuName = "Steam Eagles/Storms/Storm Config Installer", fileName = "new Storm Config", order = 0)]


[Obsolete("Use Weather.WeatherResourceInstaller instead")]
public class StormFactoryConfigInstaller : ScriptableObjectInstaller
{
    [SerializeField,Required, ValidateInput(nameof(ValidateViewPrefab))]
    private StormView stormViewPrefab;
    
    [Required]
    public StormFactoryConfig factoryConfig;
    
    
    public override void InstallBindings()
    {
       // Container.Bind<IStormFactory>().To<StormFactoryConfig>().FromInstance(factoryConfig).AsSingle();
       //Container.QueueForInject(factoryConfig);
       // Container.BindFactory<StormView, StormView.Factory>()
       //     .FromSubContainerResolve()
       //     .ByNewContextPrefab(stormViewPrefab);
    }
    
    bool ValidateViewPrefab(StormView stormView, ref string errorMessage)
    {
        if (stormView == null)
        {
            return false;
        }

        var context = stormView.GetComponent<GameObjectContext>();
        if (context== null)
        {
            errorMessage = "Missing GameObject Context";
            return false;
        }

        var regularInstallers = context.Installers.ToArray();
        var prefabInstallers = context.InstallerPrefabs.ToArray();
        int installers = regularInstallers.Length + prefabInstallers.Length +
                         context.ScriptableObjectInstallers.Count();
        if (installers == 0)
        {
            errorMessage = "Missing Zenject Installers on Storm View GOContext";
            return false;
        }

      
        foreach (var regularInstaller in regularInstallers)
        {
            if (regularInstaller is StormInstaller)
                return true;
        }

        foreach (var prefabInstaller in prefabInstallers)
        {
            if (prefabInstaller is StormInstaller)
                return true;
        }

        errorMessage = "Missing Storm Installer on Storm View GOContext";
        return false;
    }
}