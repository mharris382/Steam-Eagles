using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Buildings.Rooms;
using Cinemachine;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class RoomCamerasTable : IBuildingTable
    {
        private readonly Building _building;
        private readonly StructureState _structure;
        
        
        private Rooms.Rooms _rooms;
        public bool IsValid => _building != null && _rooms != null;
        [ValidateInput(nameof(ValidateCameras), messageType: InfoMessageType.Warning)]
        [TableList(AlwaysExpanded = true, NumberOfItemsPerPage = 10)]
        [ShowInInspector]
        private List<RoomCameraWrapper> _roomList;

        [BoxGroup("Clip Planes")]
        [ShowInInspector]
        public float standardNearClipPlane = 0.3f;
        [BoxGroup("Clip Planes")]
        [ShowInInspector]
        public float standardFarClipPlane = 1000f;

        [BoxGroup("Clip Planes")]
        [Button]
        public void StandardizeClipPlanes()
        {
            foreach (var roomCamera in _roomList)
            {
                if (roomCamera.HasCamera)
                {
                    var vcam = roomCamera.VCam;
                    vcam.m_Lens.NearClipPlane = standardNearClipPlane;
                    vcam.m_Lens.FarClipPlane = standardFarClipPlane;
                }
            }
        }

        private bool ValidateCameras(List<RoomCameraWrapper> wrappers, ref string error)
        {
            Dictionary<float, List<CinemachineVirtualCamera>> nearClipPlanes = new Dictionary<float, List<CinemachineVirtualCamera>>();
            Dictionary<float, List<CinemachineVirtualCamera>> farClipPlanes = new Dictionary<float, List<CinemachineVirtualCamera>>();
            
            foreach (var roomCameraWrapper in wrappers)
            {
                if(roomCameraWrapper.HasCamera==false)
                {
                    var vCam = roomCameraWrapper.VCam;
                    var nearClipPlane = vCam.m_Lens.NearClipPlane;
                    var farClipPlane = vCam.m_Lens.FarClipPlane;
                    if (!nearClipPlanes.TryGetValue(nearClipPlane, out var list))
                    {
                        list = new List<CinemachineVirtualCamera>();
                        nearClipPlanes.Add(nearClipPlane, list);
                    }
                    list.Add(roomCameraWrapper.VCam);
                    if (!farClipPlanes.TryGetValue(farClipPlane, out var list2))
                    {
                        list2 = new List<CinemachineVirtualCamera>();
                        farClipPlanes.Add(farClipPlane, list2);
                    }
                    list2.Add(roomCameraWrapper.VCam);
                    continue;
                }
            }

            if (nearClipPlanes.Count > 1)
            {
                error = "Non-standard near clip planes detected: " + string.Join(", ", nearClipPlanes.Keys.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToArray());
                return false;
            }

            if (farClipPlanes.Count > 1)
            {
                error = "Non-standard far clip planes detected: " + string.Join(", ", farClipPlanes.Keys.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToArray());
                return false;
            }
            return true;
        }
        

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
                get => _room == null ? "NULL" : _room.name;
                set
                {
                    if(_room != null)
                        _room.name = value;
                }
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

            public CinemachineVirtualCamera VCam => RoomVirtualCamera;
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