﻿using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class CinematicsInstaller : MonoInstaller
    {
        [Required] public GameObject defaultVCam;


        public override void InstallBindings()
        {
            Container.Bind<GameObject>().FromInstance(defaultVCam).AsSingle().WhenInjectedInto<RoomCameraLookup>().NonLazy();
            Container.BindInterfacesAndSelfTo<RoomCameraLookup>().FromNew().AsSingle().NonLazy();
            Container.Bind<PCRoomTracker>().FromNew().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PCTracker>().FromNew().AsSingle().NonLazy();
        }
    }
}