using System;
using System.Collections.Generic;
using CoreLib.SharedVariables;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Buildings.Rooms.Tracking
{
    public interface IRoomCameraSystem
    {
        void OnRoomChanged(Room previousRoom, Room newRoom);
        void OnCameraChanged([CanBeNull] GameObject prevCameraGo, [CanBeNull] GameObject newCameraGo)
        {
            prevCameraGo?.SetActive(false);
            newCameraGo?.SetActive(true);
        }
        void UpdateCamera(GameObject cameraGo, GameObject focusSubject);
    }

    public class RoomCameraLookup : IInitializable, ITickable
    {
        public class RoomCamera
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
                    return CreateVCamForPlayerFromTemplate(room.roomCamera, playerNumber,
                        _roomCameraLookup.sharedTransforms[playerNumber].Value);
                return _roomCameraLookup.GetDefaultCamera(playerNumber);
            }

            public RoomCamera(Room room, RoomCameraLookup roomCameraLookup)
            {
                this.room = room;
                _playerVCams = new GameObject[2];

                _roomCameraLookup = roomCameraLookup;
            }
        }


        private readonly Dictionary<Room, RoomCamera> _lookup;
        private readonly GameObject[] _defaultCameras;
        private PlayerRoomCameras[] _playerRoomCameras;
        private PCTracker _pcRoomTracker;
        internal List<SharedTransform> sharedTransforms;
        private List<IRoomCameraSystem> _cameraSystems;

        public bool inited { get; private set; }

        [Inject]
        public void InjectRoomCameraSystems(List<IRoomCameraSystem> cameraSystems)
        {
            _cameraSystems = cameraSystems;
        }

        [Inject]
        public void InjectDefaultCamera(GameObject defaultVCamera, PCTracker pcRoomTracker,
            List<SharedTransform> sharedTransforms)
        {
            this.sharedTransforms = sharedTransforms;
            inited = true;
            for (int i = 0; i < _defaultCameras.Length; i++)
            {
                _defaultCameras[i] = CreateVCamForPlayerFromTemplate(defaultVCamera, i, sharedTransforms[i].Value);
            }

            _playerRoomCameras = new PlayerRoomCameras[2];
            _pcRoomTracker = pcRoomTracker;
        }

        public RoomCameraLookup(GameObject defaultVCamera, PCTracker pcRoomTracker,
            List<SharedTransform> sharedTransforms)
        {
            Debug.Log("Creating RoomCameraLookup");
            _defaultCameras = new GameObject[2];
            _lookup = new Dictionary<Room, RoomCamera>();
        }

        public GameObject GetPlayerVCam(Room room, int playerNumber) => room != null
            ? GetRoomCamera(room).GetPlayerVCam(playerNumber)
            : GetDefaultCamera(playerNumber);

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

        public GameObject GetDefaultCamera(int playerNumber)
        {
            Debug.Assert(_defaultCameras != null && _defaultCameras.Length > playerNumber,
                $"Default Cameras not initialized for player {playerNumber}");
            return _defaultCameras[playerNumber];
        }

        public static GameObject CreateVCamForPlayerFromTemplate(GameObject vCamTemplate, int playerNumber,
            Transform transform)
        {
            var vCam = Object.Instantiate(vCamTemplate, vCamTemplate.transform.parent);
            vCam.name = $"{vCamTemplate.name}";
            vCam.layer = LayerMask.NameToLayer($"P{playerNumber + 1}");
            if (vCamTemplate.CompareTag("Follow Cam"))
            {
                var cinemachineType = Type.GetType("Cinemachine.CinemachineVirtualCameraBase, Cinemachine");
                if (cinemachineType == null)
                {
                    Debug.LogError("Cinemachine type not found");
                    return vCam;
                }

                var component = vCam.GetComponent(cinemachineType);
                if (component == null)
                {
                    Debug.LogError("Component not found", vCamTemplate);
                    return vCam;
                }

                cinemachineType.GetField("m_Follow").SetValue(component, transform);
            }

            return vCam;
        }


        /// <summary>
        /// tracks the room the player is in and activates the correct camera
        /// </summary>
        public class PlayerRoomCameras
        {
            private readonly RoomCameraLookup _cameraLookup;
            private readonly int _player;
            private RoomCamera _defaultCamera;
            private readonly ReactiveProperty<Room> _playerRoom;
            private readonly Dictionary<Room, RoomCamera> _roomCameras;

            
            private RoomCamera _camera;

            public IReadOnlyReactiveProperty<Room> PlayerRoom => _playerRoom;
            public PlayerRoomCameras(RoomCameraLookup cameraLookup, int player)
            {
                _cameraLookup = cameraLookup;
                _player = player;
                _roomCameras = new Dictionary<Room, RoomCamera>();
                _playerRoom = new ReactiveProperty<Room>();
            }

            private GameObject DefaultCamera => _cameraLookup.GetDefaultCamera(_player);


            private RoomCamera GetCamera(Room r)
            {
                if (r == null)
                {
                    _defaultCamera ??= _cameraLookup.GetDefaultCamera(_player).GetComponent<RoomCamera>();
                    return _defaultCamera;
                }

                if (!_roomCameras.TryGetValue(r, out var roomCamera))
                    _roomCameras.Add(r, roomCamera = new RoomCamera(r, this, _cameraLookup, _player));
                return roomCamera;
            }

            /// <summary>
            /// handles the activation/deactivation of player cameras for a specific room
            /// </summary>
            private class RoomCamera
            {
                private readonly Room _room;
                private readonly RoomCameraLookup _lookup;
                private readonly int _player;
                private GameObject vCam;
                private readonly CompositeDisposable _cd;

                public GameObject VCam => vCam;
                public int PlayerNumber => _player;

                public Room Room => _room;

                public RoomCamera(Room room, PlayerRoomCameras playerRoomCameras, RoomCameraLookup lookup, int player)
                {
                    _room = room;
                    _lookup = lookup;
                    _player = player;
                    if (room.roomCamera != null)
                    {
                        vCam = CreateVCamForPlayerFromTemplate(room.roomCamera, player,
                            lookup.sharedTransforms[player].Value);
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

        private void CreatePC(PCTracker.TrackedPC trackedPC)
        {
            if (trackedPC == null)
            {
                return;
            }

            var playerNumber = trackedPC.PlayerNumber;
            _playerRoomCameras[playerNumber] = new PlayerRoomCameras(this, playerNumber);
        }

        private void ClearPC(int playerNumber)
        {
            Debug.LogWarning("Clearing PC Not Implemented");
        }

        private Room[] _lastRooms = new Room[2];
        private GameObject[] _lastCamera = new GameObject[2];
        
        public void Tick()
        {
            // foreach (var roomCameraSystem in _cameraSystems)
            // {
            //     foreach ((int playerNumber, PCTracker.TrackedPC pc) p in _pcRoomTracker.AllPCsAndPlayerNumbers())
            //     {
            //         var subject = p.pc.Instance.character;
            //         var room = _playerRoomCameras[p.playerNumber].PlayerRoom.Value;
            //         var camera = GetRoomCamera(room);
            //         var cameraGameObject = camera.GetPlayerVCam(p.pc.PlayerNumber);
            //         
            //         roomCameraSystem.UpdateCamera(cameraGameObject, subject);
            //         
            //         if (cameraGameObject != _lastCamera[p.playerNumber])
            //         {
            //             roomCameraSystem.OnCameraChanged(_lastCamera[p.playerNumber], cameraGameObject);
            //             _lastCamera[p.playerNumber] = cameraGameObject;
            //         }
            //
            //         if (room != _lastRooms[p.playerNumber])
            //         {
            //             roomCameraSystem.OnRoomChanged(_lastRooms[p.playerNumber], room);
            //             _lastRooms[p.playerNumber] = room;
            //         }
            //     }
            // }
        }
    }

    public struct CameraChangeInfo
    {
        public readonly GameObject previousCamera;
        public readonly GameObject newCamera;


        public readonly bool isFollowCamera;
        public readonly int playerNumber;
    }
}