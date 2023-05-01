using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.Messages;
using CoreLib;
using CoreLib.Entities;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class RoomCinematicsController : MonoBehaviour
    {
        private CompositeDisposable _cd;
        private PCInstanceWrapper[] _instances;
        private RoomCameraLookup _cameraLookup;

        private class PCInstanceWrapper
        {
            public readonly PCInstance Instance;
            private readonly RoomCameraLookup _roomCameraLookup;
            public readonly int PlayerNumber;
            public Room LastRoom { get; set; }
            private GameObject _lastVCam;
            
            public PCInstanceWrapper(RoomCameraLookup roomCameraLookup, PCInstance instance, int num)
            {
                Instance = instance;
                _roomCameraLookup = roomCameraLookup;
                PlayerNumber = num;
            }

            private GameObject GetVCam(Room room) => _roomCameraLookup.GetPlayerVCam(room, PlayerNumber);

            public void EnableDefaultCamera() => SetupVCam(_roomCameraLookup.GetDefaultCamera(PlayerNumber));

            public void DisableCamerasFor(Room room) => CleanupLastVCam();

            public void EnableCamerasForRoom(Room newRoom) => SetupVCam(_roomCameraLookup.GetPlayerVCam(newRoom, PlayerNumber));

            private void SetupVCam(GameObject vCam)
            {
                CleanupLastVCam();
                vCam.SetActive(true);
                _lastVCam = vCam;
            }

            private void CleanupLastVCam()
            {
                if (_lastVCam != null)
                {
                    _lastVCam.SetActive(false);
                    _lastVCam = null;
                }
            }
        }
        
        [Inject]
        public void Install(RoomCameraLookup cameraLookup)
        {
            _cameraLookup = cameraLookup;
        }
        
        
        private void OnEnable()
        {
            MessageBroker.Default.Receive<PCInstanceChangedInfo>().Subscribe(t => SetPCInstance(t.pcInstance, t.playerNumber)).AddTo(_cd);
            MessageBroker.Default.Receive<EntityChangedRoomMessage>().Select(t => (t, GetPc(t.Entity)))
                .Where(t => t.Item2 != null).Subscribe(t => OnPcChangedRooms(t.t.Room, t.Item2));
        }

        private void OnPcChangedRooms(Room tRoom, PCInstanceWrapper pcInstance)
        {
            var prevRoom = pcInstance.LastRoom;
            if (prevRoom != null) CleanupRoomCinematics(prevRoom, pcInstance);
            var newRoom = tRoom;
            if (newRoom != null) SetupRoomCinematics(newRoom, pcInstance);
            else SetupDefaultCinematics(pcInstance);
            pcInstance.LastRoom = newRoom;
        }

        private void SetupDefaultCinematics(PCInstanceWrapper pcInstance) => pcInstance.EnableDefaultCamera();

        private static void SetupRoomCinematics(Room newRoom, PCInstanceWrapper pcInstance) => pcInstance.EnableCamerasForRoom(newRoom);

        private void CleanupRoomCinematics(Room prevRoom, PCInstanceWrapper pcInstance) => pcInstance.DisableCamerasFor(prevRoom);

        private PCInstanceWrapper GetPc(Entity entity)
        {
            if (entity == null)
                return null;
            if (entity.LinkedGameObject != null)
                return null;
            foreach (var pc in GetPcsWrappers())  
            {
                if(entity.LinkedGameObject == pc.Instance.character)
                    return pc;
            }
            return null;
        }

        private void SetPCInstance(PCInstance instance, int num)
        {
            if (HasResources())
            {
                _instances[num] = new PCInstanceWrapper(_cameraLookup, instance, num);
            }
            else
            {
                StartCoroutine(WaitForResources(instance, num));
            }
        }

        private IEnumerator WaitForResources(PCInstance instance, int num)
        {
            while (!HasResources())
            {
                yield return null;
            }
            SetPCInstance(instance, num);
        }

        private void OnDisable() => _cd?.Dispose();

        private void Awake()
        {
            _instances = new PCInstanceWrapper[2] { null, null };
            _cd = new CompositeDisposable();
        }


        private IEnumerable<PCInstanceWrapper> GetPcsWrappers()
        {
            foreach (var pcInstance in _instances)
            {
                if (pcInstance != null)
                    yield return pcInstance;
            }
        }
        public bool HasResources() => _cameraLookup != null;
        
        
    }

    
}