using System.Linq;
using SaveLoad;
using UnityEngine;
using Zenject;

namespace Buildings.DI
{
    public class SaveLoaderInstaller : Installer<ContainerLevel, SaveLoaderInstaller>
    {
        [Inject]
        public ContainerLevel containerLevel;
        public override void InstallBindings()
        {
            var concreteTypes = ReflectionUtils.GetConcreteTypes<ILayerSpecificRoomTexSaveLoader>();
            
            foreach (var concreteType in concreteTypes)
            {
                var containerLevel = Enumerable.Select<object, ContainerLevelAttribute>(concreteType.GetCustomAttributes(typeof(ContainerLevelAttribute), true), t => (ContainerLevelAttribute)t).FirstOrDefault();
                if (containerLevel.Level == this.containerLevel)
                {
                    Debug.Log($"Bound ILayerSpecificRoomTexSaveLoader to {concreteType.Name}");
                    Container.Bind<ILayerSpecificRoomTexSaveLoader>().To(concreteType).AsSingle().NonLazy();
                }

            }
        }
    }
}