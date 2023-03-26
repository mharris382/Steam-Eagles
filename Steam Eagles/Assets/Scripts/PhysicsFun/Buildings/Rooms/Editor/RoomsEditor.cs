
using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace PhysicsFun.Buildings.Rooms
{
    [CustomEditor(typeof(Rooms))]
    public class RoomsEditor : OdinEditor
    {
        NewRoomDrawer _newRoomDrawer;
        private Dictionary<Room, Editor> _editors;

        protected override void OnEnable()
        {
            _newRoomDrawer = new NewRoomDrawer(this.target as Rooms);
            _editors = new Dictionary<Room, Editor>();
            foreach (var room in (target as Rooms).AllRooms)
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
        }


        private void OnSceneGUI()
        {
            Rooms rooms = (Rooms)target;
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
            room.roomColor = Color.HSVToRGB(UnityEngine.Random.value, 1, 1);
            room.roomBounds = new Bounds(center, roomArea.size);
            roomGo.transform.SetParent(((Rooms)target).transform);

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
            var rooms = ((Rooms)target);
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
                        var rooms = _room.GetComponentInParent<Rooms>();
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
                room.roomBounds = bounds;
            }
        }
        public class RoomSizeHandle : BoxBoundsHandle
        {
            private readonly Transform _building;
            private readonly Room _room;
            public bool IsValid { get; }
            public RoomSizeHandle(Rooms buildingRooms, Room room)
            {
                IsValid = buildingRooms.HasBuilding;
                if(!IsValid) return;
                this._building = buildingRooms.Building.transform;
                this._room = room;
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
                center = _building.TransformPoint(room.roomBounds.center);
                size = _building.TransformVector(room.roomBounds.size);
                handleColor = room.roomColor;
                wireframeColor = room.roomColor;
                midpointHandleDrawFunction = Handles.DotHandleCap;
                midpointHandleSizeFunction = PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction;
            }
            
            public Vector3 GetLocalCenter()
            {
                return _building.InverseTransformPoint(center);
            }
            
            public Vector3 GetLocalSize()
            {
                return _building.InverseTransformVector(size);
            }
        }
        
        
        
        public class NewRoomDrawer 
        {
            public NewRoomDrawer(Rooms rooms)
            {
                _rooms = rooms;
            }
            
            public bool IsFinished {
                get; 
                private set;
            }

            private bool _isDrawing;
            public bool IsDrawing
            {
                get => _isDrawing;
                set
                {
                    if (_isDrawing != value)
                    {
                        _isDrawing = value;
                        if (value)
                        {
                            SceneView.beforeSceneGui += BeforeSceneGUI;
                        }
                        else
                        {
                            SceneView.beforeSceneGui -= BeforeSceneGUI;
                        }
                    }
                }
            }

            private void BeforeSceneGUI(SceneView obj)
            {
                var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                position.z = 0;
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    DoConfirmButton(position);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    DoCancelButton();
                    Event.current.Use();
                }
                else
                {
                    DoPreviewAction(position);
                }
            }

            public bool IsDrawingSecondPoint { get; private set; }
            
            public Vector3 FirstPoint { get; private set; }
            public Vector3 SecondPoint { get; private set; }
            
            public Vector3 SelectedPoint { get; private set; }
            
            

            public Vector3 FirstPointLocal => _rooms.Building.transform.InverseTransformPoint(FirstPoint);

            public Rect GetWorldSpaceRect()
            {
                if (!IsFinished)
                {
                    throw new Exception();
                }

                var minX = Mathf.Min(FirstPoint.x, SecondPoint.x);
                var minY = Mathf.Min(FirstPoint.y, SecondPoint.y);
                var maxX = Mathf.Max(FirstPoint.x, SecondPoint.x);
                var maxY = Mathf.Max(FirstPoint.y, SecondPoint.y);
                return Rect.MinMaxRect(minX, minY, maxX, maxY);
                var center = (FirstPoint + SecondPoint) / 2f;
                var size = SecondPoint - FirstPoint;
                return new Rect(center, size);
            }

            private Bounds GetLocalSpaceBounds()
            {
                var buildingTransform = _rooms.Building.transform;
                var wsRect = GetWorldSpaceRect();
                var center = buildingTransform.InverseTransformPoint(wsRect.center);
                var bounds = new Bounds(center, wsRect.size);
                return bounds;
            }
            private Bounds GetLocalSpaceBounds(Vector3 firstPoint, Vector3 secondPoint)
            {
                var minX = Mathf.Min(firstPoint.x, secondPoint.x);
                var minY = Mathf.Min(firstPoint.y, secondPoint.y);
                var maxX = Mathf.Max(firstPoint.x, secondPoint.x);
                var maxY = Mathf.Max(firstPoint.y, secondPoint.y);
                var rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
                var buildingTransform = _rooms.Building.transform;
                var wsRect = rect;
                var center = buildingTransform.InverseTransformPoint(wsRect.center);
                var bounds = new Bounds(center, wsRect.size);
                return bounds;
            }
            private Bounds GetBounds(Vector3 firstPoint, Vector3 secondPoint)
            {
                var minX = Mathf.Min(firstPoint.x, secondPoint.x);
                var minY = Mathf.Min(firstPoint.y, secondPoint.y);
                var maxX = Mathf.Max(firstPoint.x, secondPoint.x);
                var maxY = Mathf.Max(firstPoint.y, secondPoint.y);
                var rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
                var bounds = new Bounds((firstPoint + secondPoint)/2f, (firstPoint - secondPoint));
                return bounds;
            }
            private readonly Rooms _rooms;
            public bool IsValid => _rooms.HasBuilding;


            public void OnSceneGUI()
            {
                var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                position.z = 0;
                DoPreviewAction(position);
            }


            private void DoPreviewAction(Vector3 position)
            {
                void DrawSelectedPoint(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(validPoint, Vector3.forward, 0.5f);
                }
                void DrawSelectedArea(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(FirstPoint, Vector3.forward, 0.5f);
                    Handles.DrawWireDisc(validPoint, Vector3.forward, 0.5f);
                    Vector3[] verts = new Vector3[4]
                    {
                        new Vector3(FirstPoint.x, FirstPoint.y, 0),
                        new Vector3(FirstPoint.x, position.y, 0),
                        new Vector3(position.x, position.y, 0),
                        new Vector3(position.x, FirstPoint.y, 0),
                    };
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 0, 0, 0.2f), Color.red.Lighten(0.5f));
                }

                using (new HandlesScope(Color.red))
                {
                    var validPoint = position;
                    if (IsDrawingSecondPoint)
                    {
                        DrawSelectedArea(validPoint);
                    }
                    else
                    {
                        DrawSelectedPoint(validPoint);
                    }
                }
            }

            private Vector3 Local(Vector3 position)
            {
                return _rooms.Building.transform.InverseTransformPoint(position);
            }

            private Vector3 World(Vector3 position)
            {
                return _rooms.Building.transform.TransformPoint(position);
            }
            
            
            
            private void DoConfirmButton(Vector3 position)
            {
                if (!IsDrawingSecondPoint)
                {
                    FirstPoint = position;
                    IsDrawingSecondPoint = true;
                    return;
                }
                else
                {
                    SecondPoint = position;
                    IsFinished = true;
                }
            }
            
            private void DoCancelButton()
            {
                if(IsDrawingSecondPoint)
                {
                    IsDrawingSecondPoint = false;
                    return;
                }
                Dispose();
            }

            
            public void StartDrawing()
            {
                if(IsDrawing) return;
                if(!IsValid) return;
                IsDrawing = true;
                IsDrawingSecondPoint = false;
                IsFinished = false;
            }

            public void Dispose()
            {
                IsFinished = false;
                IsDrawing = false;
                IsDrawingSecondPoint = false;
            }
        }
    }

    [CustomEditor(typeof(BoundsExample)), CanEditMultipleObjects]
    public class BoundsExampleEditor : Editor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        // the OnSceneGUI callback uses the Scene view camera for drawing handles by default
        protected virtual void OnSceneGUI()
        {
            BoundsExample boundsExample = (BoundsExample)target;

            // copy the target object's data to the handle
            m_BoundsHandle.center = boundsExample.bounds.center;
            m_BoundsHandle.size = boundsExample.bounds.size;
            m_BoundsHandle.midpointHandleSizeFunction = MidpointHandleSizeFunction;
            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(boundsExample, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                boundsExample.bounds = newBounds;
                
            }
        }

        static float MidpointHandleSizeFunction(Vector3 position)
        {
            return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(position) * 2f;
        }
    }
}

#endif