#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    public partial class RoomsEditor
    {
        private class NewRoomPopupWindow
        {
            [ShowInInspector]
            private readonly Room _room;


            private bool _selectingCameraToShare;

            private bool selectingCameraToShare
            {
                get => _selectingCameraToShare;
                set
                {
                    if (value != _selectingCameraToShare)
                    {
                        _selectingCameraToShare = value;
                        if (_selectingCameraToShare)
                        {
                            SceneView.beforeSceneGui += OnSceneGUI;
                        }
                        else
                        {
                            SceneView.beforeSceneGui -= OnSceneGUI;
                        }
                    }
                }
            }

            private void OnSceneGUI(SceneView obj)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        var position = ray.origin;
                        var rooms = _room.GetComponentInParent<global::Buildings.Rooms.Rooms>();
                        var lsPosition = rooms.Building.transform.InverseTransformPoint(position);
                        bool isInRoom = false;
                        Room selectedRoom = null;
                        foreach (var roomsAllRoom in rooms.AllRooms)
                        {
                            var localSpaceBonds = roomsAllRoom.roomBounds;
                            //ignore z position
                            lsPosition.z = localSpaceBonds.center.z;
                            if (localSpaceBonds.Contains(lsPosition))
                            {
                                isInRoom = true;
                                selectedRoom = roomsAllRoom;
                                break;
                            }
                        }
                        if(!isInRoom) return;
                        if (selectedRoom.roomColor != null)
                        {
                            _room.roomCamera = selectedRoom.roomCamera;
                            _room.roomColor = selectedRoom.roomColor;
                        }
                        Event.current.Use();
                        selectingCameraToShare = false;
                    }
                    if (Event.current.button == 1)
                    {
                        selectingCameraToShare = false;
                        Event.current.Use();
                        selectingCameraToShare = false;
                    }
                }
            }

            [ShowInInspector]
            public string RoomName
            {
                get => _room.name;
                set => _room.name = value;
            }

            private bool HasCamera => _room.roomCamera != null;

            [HorizontalGroup("h1")]
            [ShowInInspector]
            public GameObject Camera
            {
                get => _room.roomCamera;
                set => _room.roomCamera = value;
            }
            
            [HorizontalGroup("h1")]
            [Button("Create Camera"), HideIf(nameof(HasCamera)), DisableInPlayMode]
            public void CreateCinemachineCamera()
            {
                var canGo = new GameObject($"{RoomName} virtual camera");
                Undo.RegisterCreatedObjectUndo(canGo, "Created Cinemachine Camera");
                var cam = canGo.AddComponent(Type.GetType("Cinemachine.CinemachineVirtualCamera"));
                canGo.transform.SetParent(_room.transform);
                canGo.transform.position = _room.roomBounds.center;
                OdinMenuEditorWindow.InspectObject(canGo);
            }


            void SelectCameraToShare()
            {
                
            }
            
            
            

            public NewRoomPopupWindow(Room room)
            {
                this._room = room;
            }
        }
    }
}
#endif