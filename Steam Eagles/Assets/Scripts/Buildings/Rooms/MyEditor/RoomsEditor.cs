#if UNITY_EDITOR
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    [CustomEditor(typeof(Rooms))]
    public partial class RoomsEditor : OdinEditor
    {
        NewRoomDrawer _newRoomDrawer;
        private Dictionary<Room, Editor> _editors;
        private SerializedProperty _roomGroupingsProperty;

        private RoomsGraphEditor _graphEditor;
        private Dictionary<Color, Rooms.RoomGroupings> _colorToGrouping = new Dictionary<Color, Rooms.RoomGroupings>();

        private GridHelper _gridHelper;
        public GridHelper GridHelper => _gridHelper ??= new GridHelper(target as Rooms);
        protected override void OnEnable()
        {
            if (target == null || (target as Rooms)==null) return;
            _graphEditor = new RoomsGraphEditor(target as Rooms);
            _newRoomDrawer = new NewRoomDrawer(target as Rooms);
            _editors = new Dictionary<Room, Editor>();
            _roomGroupingsProperty = serializedObject.FindProperty("roomGroupings");
            var groups = (target as Rooms)?.GetGroups(true);
            var allRooms = (target as Rooms)?.AllRooms;
            if (allRooms != null)
                foreach (var room in allRooms)
                {
                    _editors.Add(room, CreateEditor(room, typeof(RoomEditor)));
                }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            _graphEditor.Cleanup();
            _editors.Clear();
            if(_newRoomDrawer != null){
                _newRoomDrawer.IsDrawing = false;
            }
            isCopyingColor = false;
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
            
            _graphEditor.OnInspectorGUI();
            
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

        private void OnPreSceneGUI()
        {
            Rooms rooms = (Rooms)target;
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            float size = HandleUtility.GetHandleSize(HandleUtility.WorldToGUIPoint(Event.current.mousePosition)) * 0.1f;
            _graphEditor.OnPreSceneGUI(size);
        }

        private void OnSceneGUI()
        {
            Rooms rooms = (Rooms)target;
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;

            float size = HandleUtility.GetHandleSize(HandleUtility.WorldToGUIPoint(Event.current.mousePosition))* 0.1f;
            _graphEditor.OnPreSceneGUI(size);
            _graphEditor.OnSceneGUI(size);
            
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

            if (_graphEditor.IsEditing) return;
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

            
        }

        private void CreateNewRoom(Rect roomArea)
        {
            var targetRooms = (Rooms)target;
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
            room.roomColor = Color.HSVToRGB(Random.value, 1, 1);
            room.RoomBounds = new Bounds(center, roomArea.size);
            roomGo.transform.SetParent(((Rooms)target).transform);

            targetRooms.UpdateRoomsList();
            _newRoomDrawer.Dispose();
            var btnRect = GUIHelper.GetCurrentLayoutRect();
            OdinEditorWindow.InspectObject(new NewRoomWindow(room));
        }

        public class NewRoomWindow
        {
            private readonly Room _room;

            [ShowInInspector]
            public string roomName
            {
                get => _room.name;
                set => _room.name = value;
            }

            [ShowInInspector, ColorPalette]
            public Color roomColor
            {
                get => _room.roomColor;
                set => _room.roomColor = value;
            }
            
            [ShowInInspector]
            public CameraHelper cameraHelper;

            [ShowInInspector]
            public BuildLevel BuildLevel
            {
                get => _room.buildLevel;
                set => _room.buildLevel = value;
            }
            
            [ShowInInspector]
            public AccessLevel accessLevel
            {
                get => _room.accessLevel;
                set => _room.accessLevel = value;
            }

            public NewRoomWindow(Room room)
            {
                _room = room;
                cameraHelper = new CameraHelper(room);
            }
        }
        
        [InlineProperty()]
        public class CameraHelper
        {
            public Room room;
            public bool useExistingCamera;

            
            [ShowInInspector]
            public bool IsFollowCamera
            {
                get => room.roomCameraConfig.IsDynamic;
                set => room.roomCameraConfig.IsDynamic = value;
            }
            
            [ShowInInspector, HideIf(nameof(useExistingCamera))]
            public GameObject RoomCamera
            {
                get => room.roomCamera;
                set => room.roomCamera = value;
            }
            [ShowInInspector, ShowIf(nameof(useExistingCamera))]
            [ValueDropdown(nameof(ExistingCameras))]
            public GameObject SharedRoomCamera
            {
                get => RoomCamera;
                set => RoomCamera = value;
            }
            ValueDropdownList<GameObject> ExistingCameras()
            {
                var vdl = new ValueDropdownList<GameObject>();
                var rooms = room.GetComponentInParent<Rooms>();
                HashSet<GameObject> cameras = GetAllCameras();
                foreach (var gameObject in cameras) vdl.Add(gameObject.name, gameObject);
                return vdl;
            }

            HashSet<GameObject> GetAllCameras()
            {
                HashSet<GameObject> seenCameras = new HashSet<GameObject>();
                var rooms = room.GetComponentInParent<Rooms>();
                foreach (var roomsAllRoom in rooms.AllRooms)
                {
                    var cam = roomsAllRoom.roomCamera;
                    if(cam == null) continue;
                    if(seenCameras.Contains(cam)) continue;
                    seenCameras.Add(cam);
                }
                return seenCameras;
            }
            public CameraHelper(Room room)
            {
                this.room = room;
                if(room.roomCamera != null) useExistingCamera = true;
            }
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
            var rooms = ((Rooms)target);
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            void DrawCopyTo()
            {
                var fromPosition =  rooms.Building.transform.TransformPoint(_copyingFrom.RoomBounds.center);
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
                    var toPosition = rooms.Building.transform.TransformPoint(toRoom.RoomBounds.center);
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
                    var rooms = ((Rooms)target);
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


        private void DrawRoomHandle(Rooms rooms, Room room)
        {
            var handle = new RoomSizeHandle(rooms, room);

            EditorGUI.BeginChangeCheck();
            if (handle.IsValid)
                handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rooms, "Changed Room Size");
                var bounds = new Bounds(handle.GetLocalCenter(), handle.GetLocalSize());
                room.RoomBounds = bounds;
            }
        }
    }
}

#endif