using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class RoomCameraLookup
    {
        public const string V_CAM_INJECT_ID = "Default VCam";
        private readonly Dictionary<Room, RoomCamera> _lookup;
        private readonly GameObject[] _defaultCameras;
        
        public GameObject GetPlayerVCam(Room room, int playerNumber) => GetRoomCamera(room).GetPlayerVCam(playerNumber);


        public RoomCameraLookup([Inject(Id = V_CAM_INJECT_ID)] GameObject defaultVCamera)
        {
            _defaultCameras = new GameObject[2];
            for (int i = 0; i < _defaultCameras.Length; i++)
            {
                _defaultCameras[i] = CreateVCamForPlayerFromTemplate(defaultVCamera, i);
            }
        }
        private RoomCamera GetRoomCamera(Room room)
        {
            if (_lookup.TryGetValue(room, out var roomCamera))
            {
                roomCamera = new RoomCamera(room, this);
                _lookup.Add(room, roomCamera);
            }
            return roomCamera;
        }
        public RoomCameraLookup() => _lookup = new Dictionary<Room, RoomCamera>();

        private class RoomCamera
        {
            public Room room;
            private readonly GameObject[] _playerVCams;
            private readonly RoomCameraLookup _roomCameraLookup;

            public GameObject GetPlayerVCam(int playerNumber)
            {
                var vCam = _playerVCams[playerNumber];
                if (vCam == null)
                {
                    vCam = CreatePlayerVCam(playerNumber);
                    _playerVCams[playerNumber] = vCam;
                }
                return vCam;
            }

            private GameObject CreatePlayerVCam(int playerNumber)
            {
                if (room.roomCamera != null)
                    return CreateVCamForPlayerFromTemplate(room.roomCamera, playerNumber);
                return _roomCameraLookup.GetDefaultCamera(playerNumber);
            }
            public RoomCamera(Room room, RoomCameraLookup roomCameraLookup)
            {
                this.room = room;
                _playerVCams = new GameObject[2];
                _roomCameraLookup = roomCameraLookup;
            }
        }

        public GameObject GetDefaultCamera(int playerNumber) => _defaultCameras[playerNumber];

        public static GameObject CreateVCamForPlayerFromTemplate(GameObject vCamTemplate, int playerNumber)
        {
            var vCam = Object.Instantiate(vCamTemplate, vCamTemplate.transform.parent);
            vCam.name = $"{vCamTemplate.name} P#{playerNumber}";
            vCam.layer = LayerMask.NameToLayer($"P{playerNumber + 1}");
            return vCam;
        }
    }
}