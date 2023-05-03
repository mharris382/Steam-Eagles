using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Cinemachine;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class RoomCamerasTable
    {
        private readonly Building _building;
        private readonly StructureState _structure;
        private Rooms.Rooms _rooms;
        [TableList(AlwaysExpanded = true, NumberOfItemsPerPage = 10)]
        [ShowInInspector]
        private List<RoomCameraWrapper> _roomList;
        
        

        public RoomCamerasTable(Building building)
        {
            this._building = building;
            if (!_building.gameObject.TryGetComponent(out _structure))
            {
                _structure = _building.gameObject.AddComponent<StructureState>();
            }
            _rooms = building.GetComponentInChildren<Rooms.Rooms>();
            _roomList = new List<RoomCameraWrapper>(_rooms.AllRooms.Select(t => new RoomCameraWrapper(t)));
        }
        
        
        public class RoomCameraWrapper
        {
            private readonly Room _room;

            [GUIColor(nameof(guiColor))]
            [ShowInInspector, TableColumnWidth(120), InlineButton(nameof(SelectRoom), "Select")]
            public string RoomName
            {
                get => _room.name;
                set => _room.name = value;
            }

            private Color guiColor => _room.roomColor;
            void SelectRoom()
            {
                Selection.activeGameObject = _room.gameObject;
            }
            
            private GameObject RoomCamera
            {
                get
                {
                    return _room.roomCamera;
                }
            }

            private CinemachineVirtualCamera _vCam;

            [ShowInInspector]
            public bool IsCameraDynamic
            {
                get => _room.isRoomCameraDynamic;
                set => _room.isRoomCameraDynamic = value;
            }
            
            [ShowInInspector]
            private CinemachineVirtualCamera RoomVirtualCamera
            {
                get
                {
                    if (_vCam != null)
                    {
                        if (RoomCamera == null) _vCam = null;
                        else return _vCam;
                    }

                    if (_vCam == null)
                    {
                        if (RoomCamera == null) return null;
                        _vCam = RoomCamera.GetComponent<CinemachineVirtualCamera>();
                        if (_vCam == null)
                        {
                            _vCam = RoomCamera.AddComponent<CinemachineVirtualCamera>();
                        }
                    }

                    return _vCam;
                }
            }
            
            public bool HasCamera => RoomVirtualCamera != null;

            public RoomCameraWrapper(Room room)
            {
                _room = room;
            }

            [ButtonGroup()]
            [Button(), HideIf(nameof(HasCamera))]
            public void AddCamera()
            {
                var cameraGO = new GameObject($"[{_room.name}] CAMERA");
                cameraGO.transform.SetParent(_room.transform);
                var vCam = cameraGO.AddComponent<CinemachineVirtualCamera>();
                vCam.gameObject.AddComponent<GameplayCamera>();
                _room.roomCamera = cameraGO;
                vCam.m_Priority = 16;
            }

            [ButtonGroup()]
            [Button("Select"), ShowIf(nameof(HasCamera))]
            public void SelectCamera()
            {
                Selection.activeGameObject = this.RoomCamera;
            }

            [ButtonGroup()]
            [Button("Recenter"), ShowIf(nameof(HasCamera))]
            public void RecenterCameraOnRoom()
            {
                var worldCenter = _room.WorldCenter;
                worldCenter.z += -10;
                var vCam = RoomVirtualCamera;
                vCam.transform.position = worldCenter;
               
            }

            [ButtonGroup()]
            [Button("Fit To Room"), ShowIf(nameof(HasCamera))]
            public void FitLensToRoom()
            {
                var vCam = RoomVirtualCamera;
                var lens = vCam.m_Lens;
                var bounds = _room.Bounds;
                var size = Mathf.Min(bounds.size.x, bounds.size.y);
                lens.OrthographicSize = size / 2;
                vCam.m_Lens = lens;
            }
        }
    }
}