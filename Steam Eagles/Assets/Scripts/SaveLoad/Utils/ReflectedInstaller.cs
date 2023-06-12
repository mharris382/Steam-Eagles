using System;
using UnityEngine;
using Zenject;

namespace SaveLoad
{
    public class ReflectedInstaller<TBound> : Installer<ContainerLevel, ReflectedInstaller<TBound>>
    {
        [Inject]
        public ContainerLevel containerLevel;
        public override void InstallBindings()
        {
            var concreteTypes = ReflectionUtils.GetConcreteTypes<TBound>();
            foreach (var concreteType in concreteTypes)
            {
                var containerLevelAttribute = concreteType.GetCustomAttribute<ContainerLevelAttribute>(true) ?? new ContainerLevelAttribute(ContainerLevel.PROJECT);
                if (containerLevelAttribute.Level == this.containerLevel)
                {
                    Container.Bind(concreteType).AsSingle().NonLazy();
                    Debug.Log($"Bound {typeof(TBound).Name} to {concreteType.Name} at {containerLevelAttribute.Level} level");
                }
            }
        }
    }
}