using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class CinematicsInstaller : MonoInstaller
    {
        [Required] public GameObject defaultVCam;


        public override void InstallBindings()
        {
            Container.Bind<GameObject>().FromInstance(defaultVCam).AsSingle().WhenInjectedInto<RoomCameraLookup>()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<RoomCameraLookup>().FromNew().AsSingle().NonLazy();
            Container.Bind<PCRoomTracker>().FromNew().AsSingle().NonLazy();
            //
            
            
            Container.Bind<IRoomCameraSystem>().To<TestCameraSystem1>().AsSingle().NonLazy();
            Container.Bind<IRoomCameraSystem>().To<TestCameraSystem2>().AsSingle().NonLazy();
        }
    }
    
    public class TestCameraSystem2 : IRoomCameraSystem
    {
        public void OnRoomChanged(Room previousRoom, Room newRoom)
        {
            var name1 = previousRoom != null ? previousRoom.name : "NULL";
            var name2 = newRoom != null ? newRoom.name : "NULL";
            Debug.Log($"TestCameraSystem2 RoomChanged {name1.InItalics()} to {name2.InItalics()}");
        }

        public void UpdateCamera(GameObject cameraGo, GameObject focusSubject)
        {
            var name1 = cameraGo != null ? cameraGo.name : "NULL";
            var name2 = focusSubject != null ? focusSubject.name : "NULL";
            Debug.Log($"TestCameraSystem2 Update Camera {name1.InItalics()} to {name2.InItalics()}");
        }
    }
    public class TestCameraSystem1 : IRoomCameraSystem
    {
        public void OnRoomChanged(Room previousRoom, Room newRoom)
        {
            var name1 = previousRoom != null ? previousRoom.name : "NULL";
            var name2 = newRoom != null ? newRoom.name : "NULL";
            Debug.Log($"TestCameraSystem1 RoomChanged {name1.InItalics()} to {name2.InItalics()}");
        }

        public void UpdateCamera(GameObject cameraGo, GameObject focusSubject)
        {
            var name1 = cameraGo != null ? cameraGo.name : "NULL";
            var name2 = focusSubject != null ? focusSubject.name : "NULL";
            Debug.Log($"TestCameraSystem1 Update Camera {name1.InItalics()} to {name2.InItalics()}");
        }
    }
}