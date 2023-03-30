#if UNITY_EDITOR
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    [CustomEditor(typeof(global::Buildings.Rooms.Rooms))]
    public partial class RoomsEditor : OdinEditor
    {
        NewRoomDrawer _newRoomDrawer;
        private Dictionary<Room, Editor> _editors;
        private SerializedProperty _roomGroupingsProperty;

        private Dictionary<Color, global::Buildings.Rooms.Rooms.RoomGroupings> _colorToGrouping = new Dictionary<Color, global::Buildings.Rooms.Rooms.RoomGroupings>();

        protected override void OnEnable()
        {
            _newRoomDrawer = new NewRoomDrawer(this.target as global::Buildings.Rooms.Rooms);
            _editors = new Dictionary<Room, Editor>();
            _roomGroupingsProperty = serializedObject.FindProperty("roomGroupings");
            var groups = (target as global::Buildings.Rooms.Rooms).GetGroups(true);
            foreach (var room in (target as global::Buildings.Rooms.Rooms).AllRooms)
            {
                _editors.Add(room, CreateEditor(room, typeof(RoomEditor)));
            }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            _editors.Clear();
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            if (_newRoomDrawer.IsValid)
            {
                if (!_newRoomDrawer.IsDrawing && GUILayout.Button("Add New Room"))
                {
                    _newRoomDrawer.StartDrawing();
                }
                else if (_newRoomDrawer.IsDrawing)
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        _newRoomDrawer.IsDrawing = false;
                    }
                }
                else if (!_newRoomDrawer.IsDrawing)
                {
                    _newRoomDrawer.Dispose();
                }
                //if(!RoomsEditorWindow.IsEditorOpen((Rooms) target) && GUILayout.Button("Open Rooms Editor"))
                //{
                //    RoomsEditorWindow.OpenWindow((Rooms) target);
                //}
            }
            DrawCopyColorButton();
            base.OnInspectorGUI();
            
            
            var roomTrackerManager = FindObjectOfType<RoomTrackerManager>();
            if (roomTrackerManager == null)
            {
                EditorGUILayout.HelpBox("RoomTrackerManager not found in scene", MessageType.Warning);
                if(GUILayout.Button("Create RoomTrackerManager"))
                {
                    var go = new GameObject("[ROOM TRACKER MANAGER]");
                    go.AddComponent<RoomTrackerManager>();
                    Selection.activeGameObject = go;
                }
            }
        }


        private void OnSceneGUI()
        {
            global::Buildings.Rooms.Rooms rooms = (global::Buildings.Rooms.Rooms)target;
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;

            
            Tools.hidden = _newRoomDrawer.IsDrawing || isCopyingColor;
            if (isCopyingColor)
            {
                DrawCopyColorOnScene();
            }
            if (_newRoomDrawer.IsDrawing)
            {
                _newRoomDrawer.OnSceneGUI();
                if (_newRoomDrawer.IsFinished)
                {
                    CreateNewRoom(_newRoomDrawer.GetWorldSpaceRect());
                    _newRoomDrawer.IsDrawing = false;
                    _newRoomDrawer.Dispose();
                }
            }
            else
            {
                _newRoomDrawer.Dispose();
            }

            foreach (var editor in _editors)
            {
                var roomEditor = editor.Value as RoomEditor;
                if (roomEditor != null)
                {
                    roomEditor.OnSceneGUI();
                }
            }

            foreach (var room in rooms.AllRooms)
            {
                RoomEditor.DrawRoomArea(rooms, room);

            }
        }

        private void CreateNewRoom(Rect roomArea)
        {
            var targetRooms = (global::Buildings.Rooms.Rooms)target;
            var buildingTransform = targetRooms.Building.transform;
            var centerWs = roomArea.center;
            var center = buildingTransform.InverseTransformPoint(centerWs);
            roomArea = new Rect(center, roomArea.size);
            var roomGo = new GameObject("New Room");
            Undo.RegisterCreatedObjectUndo(roomGo, "Created New Room");
            var boxCollider = roomGo.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            var room = roomGo.AddComponent<Room>();
            room.gameObject.layer = LayerMask.NameToLayer("Triggers");
            room.tag = "Room";
            room.roomColor = Color.HSVToRGB(UnityEngine.Random.value, 1, 1);
            room.roomBounds = new Bounds(center, roomArea.size);
            roomGo.transform.SetParent(((global::Buildings.Rooms.Rooms)target).transform);

            targetRooms.UpdateRoomsList();
            _newRoomDrawer.Dispose();
            var btnRect = GUIHelper.GetCurrentLayoutRect();
            OdinEditorWindow.InspectObject(roomGo);
        }


        bool _isCopyingColor;
        Room _copyingFrom;
        bool isCopyingColor
        {
            get => _isCopyingColor;
            set
            {
                   if(_isCopyingColor != value)
                   {
                       _isCopyingColor = value;
                       if(_isCopyingColor)
                       {
                           SceneView.beforeSceneGui += OnCopyColorSceneGUI;
                       }
                       else
                       {
                           SceneView.beforeSceneGui -= OnCopyColorSceneGUI;
                       }
                   }
            }
        }
        public void DrawCopyColorButton()
        {
            if (isCopyingColor)
            {
                if(GUILayout.Button("Stop Copying Color"))
                {
                    isCopyingColor = false;
                }
                
            }
            else if (GUILayout.Button("Copy Color"))
            {
                isCopyingColor = true;
            }
            
        }

        public void DrawCopyColorOnScene()
        {
            var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            var rooms = ((global::Buildings.Rooms.Rooms)target);
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            void DrawCopyTo()
            {
                var fromPosition =  rooms.Building.transform.TransformPoint(_copyingFrom.roomBounds.center);
                var fromColor = _copyingFrom.roomColor;
                using (new HandlesScope(fromColor))
                {
                    Handles.DrawWireDisc(fromPosition, Vector3.forward, 0.5f);
                    Handles.DrawLine(position, fromPosition);
                    Handles.DrawWireDisc(position, Vector3.forward, 0.5f);
                }
            }
            void DrawCopyFrom()
            {
                var toRoom = rooms.GetRoomAtWS(position);
                if (toRoom != null)
                {
                    var toPosition = rooms.Building.transform.TransformPoint(toRoom.roomBounds.center);
                    var toColor = toRoom.roomColor;
                    using (new HandlesScope(toColor))
                    {
                        Handles.DrawWireDisc(toPosition, Vector3.forward, 0.5f);
                        Handles.DrawLine(position, toPosition);
                        Handles.DrawWireDisc(position, Vector3.forward, 0.5f);
                    }
                }
                else
                {
                    Handles.DrawWireDisc(position, Vector3.forward, 0.5f);
                }
            }
            if (_copyingFrom != null)
            {
                DrawCopyTo();
            }
            else
            {
                DrawCopyFrom();
            }
        }

        public void OnCopyColorSceneGUI(SceneView view)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                    var rooms = ((global::Buildings.Rooms.Rooms)target);
                    if (_copyingFrom == null)
                    {
                        _copyingFrom = rooms.GetRoomAtWS(position);
                    }
                    else
                    {
                        var toRoom = rooms.GetRoomAtWS(position);
                        if (toRoom != null)
                        {
                            toRoom.roomColor = _copyingFrom.roomColor;
                        }
                        _copyingFrom = null;
                    }
                    Event.current.Use();
                }
            }
        }


        private void DrawRoomHandle(global::Buildings.Rooms.Rooms rooms, Room room)
        {
            var handle = new RoomSizeHandle(rooms, room);

            EditorGUI.BeginChangeCheck();
            if (handle.IsValid)
                handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rooms, "Changed Room Size");
                var bounds = new Bounds(handle.GetLocalCenter(), handle.GetLocalSize());
                room.roomBounds = bounds;
            }
        }
    }
}

#endif