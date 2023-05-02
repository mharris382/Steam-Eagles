using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    [InfoBox("Attaches to the building gameobject context")]
    public class BuildingCinematicsInstaller : MonoInstaller
    {


        public override void InstallBindings()
        {
            Debug.Log("Installing BuildingCinematicsInstaller", this);
            Container.Bind<RoomCinematicsController>().FromNewComponentOn(gameObject).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<Tester>().FromNew().AsSingle().NonLazy();
        }

        public class Tester : IInitializable
        {
            private readonly RoomCinematicsController _controller;

            public Tester(RoomCinematicsController controller)
            {
                _controller = controller;
            }

            public void Initialize()
            {
                Debug.Assert(_controller.HasResources(), _controller);
            }
        }
    }
}