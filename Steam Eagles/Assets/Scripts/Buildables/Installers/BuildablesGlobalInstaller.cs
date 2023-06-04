using Buildings;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables.Installers
{
    [InfoBox("Should be bound at the Project Context level")]
    public class BuildablesGlobalInstaller : MonoInstaller
    {
        [SerializeField] private HypergasEngineConfig hypergasEngineConfig;
        public override void InstallBindings()
        {
            Container.Bind<BuildablesRegistry>().AsSingle().NonLazy();
            Container.Bind<HypergasEngineConfig>().FromInstance(hypergasEngineConfig).AsSingle();
            
        }
    }
}