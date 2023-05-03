using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class RoomCameraLookup : IInitializable
    {
        public const string V_CAM_INJECT_ID = "Default VCam";
        
        private readonly Dictionary<Room, RoomCamera> _lookup;
        private GameObject prefabVCam;
        private readonly GameObject[] _defaultCameras;
        private PlayerRoomCameras[] _playerRoomCameras;
        private PCTracker _pcRoomTracker;
        public bool inited { get; private set; }
        public GameObject GetPlayerVCam(Room room, int playerNumber) => room != null ? GetRoomCamera(room).GetPlayerVCam(playerNumber) : GetDefaultCamera(playerNumber);


        [Inject]
        public void InjectDefaultCamera(GameObject defaultVCamera, PCTracker pcRoomTracker)
        {
            this.prefabVCam  = defaultVCamera;
            inited = true;
            for (int i = 0; i < _defaultCameras.Length; i++)
            {
                _defaultCameras[i] = CreateVCamForPlayerFromTemplate(defaultVCamera, i);
            }
            _playerRoomCameras = new PlayerRoomCameras[2];
            _pcRoomTracker = pcRoomTracker;
        }
        
        public RoomCameraLookup()
        {
            Debug.Log("Creating RoomCameraLookup");
            _defaultCameras = new GameObject[2];
            _lookup = new Dictionary<Room, RoomCamera>();
        }

        private RoomCamera GetRoomCamera(Room room)
        {
            if (!_lookup.TryGetValue(room, out var roomCamera))
            {
                roomCamera = new RoomCamera(room, this);
                _lookup.Add(room, roomCamera);
            }

            if (roomCamera == null)
            {
                _lookup.Remove(room);
                _lookup.Add(room, roomCamera = new RoomCamera(room, this));
            }
            return roomCamera;
        }

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

        public GameObject GetDefaultCamera(int playerNumber)
        {
            Debug.Assert(_defaultCameras != null && _defaultCameras.Length > playerNumber, $"Default Cameras not initialized for player {playerNumber}");
            return _defaultCameras[playerNumber];
        }

        public static GameObject CreateVCamForPlayerFromTemplate(GameObject vCamTemplate, int playerNumber)
        {
            var vCam = Object.Instantiate(vCamTemplate, vCamTemplate.transform.parent);
            vCam.name = $"{vCamTemplate.name} P#{playerNumber}";
            vCam.layer = LayerMask.NameToLayer($"P{playerNumber + 1}");
            return vCam;
        }


        public class PlayerRoomCameras
        {
            private readonly RoomCameraLookup _cameraLookup;
            private readonly int _player;
            private ReactiveProperty<Room> _playerRoom;
            private Dictionary<Room, RoomCamera> _roomCameras;


            public PlayerRoomCameras(RoomCameraLookup cameraLookup, int player)
            {
                _cameraLookup = cameraLookup;
                _player = player;
                _roomCameras = new Dictionary<Room, RoomCamera>();
                _playerRoom = new ReactiveProperty<Room>();
            }

            private GameObject DefaultCamera => _cameraLookup.GetDefaultCamera(_player);
            
            public void OnPlayerChangedRoom(Room room)
            {
                _playerRoom.Value = room;
                var camera = GetCamera(room);
                camera.Activate();
            }

            RoomCamera GetCamera(Room r)
            {
                if (r == null)
                {
                    
                }
                if(!_roomCameras.TryGetValue(r, out var roomCamera))
                    _roomCameras.Add(r, roomCamera = new RoomCamera(r, this, _cameraLookup, _player));
                return roomCamera;
            }

            private class RoomCamera
            {
                private readonly Room _room;
                private readonly RoomCameraLookup _lookup;
                private readonly int _player;
                private GameObject vCam;
                private readonly CompositeDisposable _cd;

                public RoomCamera(PlayerRoomCameras playerRoomCameras,  RoomCameraLookup lookup, int player)
                {
                    vCam = lookup.GetDefaultCamera(player);
                    _cd = new CompositeDisposable();
                    playerRoomCameras._playerRoom.Subscribe(_ => Deactivate()).AddTo(_cd);
                }
                
                public RoomCamera(Room room, PlayerRoomCameras playerRoomCameras, RoomCameraLookup lookup, int player)
                {
                    _room = room;
                    _lookup = lookup;
                    _player = player;
                    if (room.roomCamera != null)
                    {
                        vCam = CreateVCamForPlayerFromTemplate(room.roomCamera, player);
                        vCam.SetActive(false);
                    }
                    else
                    {
                        vCam = lookup.GetDefaultCamera(player);
                    }
                    _cd = new CompositeDisposable();
                    playerRoomCameras._playerRoom.Subscribe(_ => Deactivate()).AddTo(_cd);
                }

                public void Activate()
                {
                    vCam.SetActive(true);
                }
                void Deactivate()
                {
                    vCam.SetActive(false);
                }
            }
        }

        
        public void Initialize()
        {
            _pcRoomTracker.GetPCs().Subscribe(CreatePC);
            _pcRoomTracker.GetPC(0).Where(t => t == null).Select(t => 0).Subscribe(ClearPC);
            _pcRoomTracker.GetPC(1).Where(t => t == null).Select(t => 1).Subscribe(ClearPC);
        }

        private void CreatePC(PCTracker.PC pc)
        {
            if (pc == null)
            {
                return;
            }
            var playerNumber = pc.PlayerNumber;
            _playerRoomCameras[playerNumber] = new PlayerRoomCameras(this, playerNumber);
        }
        private void ClearPC(int playerNumber)
        {
            
        }
    }
}