using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Weather.Storms;
using Weather.Storms.Views;
using Zenject;

namespace Weather
{
    [InfoBox("Should be installed in project context")]
    [CreateAssetMenu(fileName = "Weather Resources Installer", menuName = "Steam Eagles/Storms/Weather Resources Installer", order = 0)]
    public class WeatherResourcesInstaller : ScriptableObjectInstaller
    {
        public GlobalStormConfig stormConfig;
        [ValidateInput(nameof(ValidateViewPrefab))]
        public StormView stormViewPrefab;

        public override void InstallBindings()
        {
            StormRegistryInstaller.Install(Container);
            Container.Bind<GlobalStormConfig>().FromInstance(stormConfig).AsSingle();
            //Container.BindFactory<StormView, StormView.Factory>().FromSubContainerResolve().ByNewContextPrefab(stormViewPrefab);
            Container.BindFactory<StormView, StormView.AltFactory>().FromSubContainerResolve().ByNewContextPrefab(stormViewPrefab).AsSingle();
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
                if (regularInstaller is StormViewInstaller)
                    return true;
            }

            foreach (var prefabInstaller in prefabInstallers)
            {
                if (prefabInstaller is StormViewInstaller)
                    return true;
            }

            errorMessage = "Missing Storm Installer on Storm View GOContext";
            return false;
        }
    }
}