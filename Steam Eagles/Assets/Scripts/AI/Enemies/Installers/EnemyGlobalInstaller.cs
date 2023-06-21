using System.Collections.Generic;
using System.Linq;
using CoreLib.Interfaces;
using CoreLib.Structures;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Installers
{
    public class EnemyGlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ITargetFinder>().To<TargetFinder>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TestTargets>().AsSingle().NonLazy();
        }

        class TargetFinder : ITargetFinder
        {
            private readonly List<ITargetProvider> _targetProviders;
            public TargetFinder(List<ITargetProvider> targetProviders)
            {
                _targetProviders = targetProviders;
            }
            public IEnumerable<Target> GetTargets() => _targetProviders.SelectMany(t => t.GetTargets());
        }
    }
}