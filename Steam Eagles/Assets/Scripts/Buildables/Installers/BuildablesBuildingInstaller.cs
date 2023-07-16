using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables.Installers
{
    [InfoBox("Should be on GameObjectContext level within Building Context")]
    public class BuildablesBuildingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<PowerTimerUtility,PowerTimerUtility.Factory>().AsSingle().NonLazy();
            // Container.BindFactory<HypergasGenerator, HypergasEngineController, HypergasEngineController.Factory>().AsSingle().NonLazy();
            // Container.BindFactory<HyperPump, HyperPumpController, HyperPumpController.Factory>().AsSingle().NonLazy();
            // Container.BindFactory<SteamTurbine, SteamTurbineController, SteamTurbineController.Factory>().AsSingle().NonLazy();
            // Container.BindFactory<ExhaustVent, ExhaustVentController, ExhaustVentController.Factory>().AsSingle().NonLazy();
        }
    }

}