
using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

namespace PhysicsFun.Buildings.Rooms
{
    [CustomEditor(typeof(Rooms))]
    public class RoomsEditor : OdinEditor
    {
        NewRoomDrawer _newRoomDrawer;
        protected override void OnEnable()
        {
            _newRoomDrawer = new NewRoomDrawer(this.target as Rooms);
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            if (_newRoomDrawer.IsValid)
            {
                if (!_newRoomDrawer.IsDrawing && GUILayout.Button("Add New Room"))
                {
                    _newRoomDrawer.StartDrawing();
                }
                else if(_newRoomDrawer.IsDrawing)
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        _newRoomDrawer.IsDrawing = false;
                    }
                }
                if(!RoomsEditorWindow.IsEditorOpen((Rooms) target) && GUILayout.Button("Open Rooms Editor"))
                {
                    RoomsEditorWindow.OpenWindow((Rooms) target);
                }
            }
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            Rooms rooms = (Rooms) target;
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
           
            Tools.hidden = _newRoomDrawer.IsDrawing;
            if (_newRoomDrawer.IsDrawing)
            {
                _newRoomDrawer.OnSceneGUI();
                if (_newRoomDrawer.IsFinished)
                {
                    CreateNewRoom(_newRoomDrawer.GetWorldSpaceRect());
                    _newRoomDrawer.IsDrawing = false;
                }
            }
            foreach (var room in rooms.AllRooms)
            {
                RoomEditor.DrawRoomArea(rooms, room);
            }
        }

        private void CreateNewRoom(Rect roomArea)
        {
            var targetRooms = (Rooms) target;
            var buildingTransform = targetRooms.Building.transform;
            var centerWs = roomArea.center;
            var centerLs = buildingTransform.TransformPoint(centerWs);
            roomArea = new Rect(centerLs, roomArea.size);
            var roomGo = new GameObject("New Room");
            var boxCollider = roomGo.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            var room = roomGo.AddComponent<Room>();
            room.gameObject.layer = LayerMask.NameToLayer("Triggers");
            room.tag = "Room";
            room.roomColor = Color.HSVToRGB(UnityEngine.Random.value, 1, 1);
            room.roomBounds = new Bounds(roomArea.center, roomArea.size);
            roomGo.transform.SetParent(((Rooms) target).transform);
            
            targetRooms.UpdateRoomsList();
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


            public Rect GetWorldSpaceRect()
            {
                if (!IsFinished)
                {
                    throw new Exception();
                }
                var center = (FirstPoint + SecondPoint) / 2f;
                var size = SecondPoint - FirstPoint;
                return new Rect(center, size);
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
                Handles.color = Color.red;
                if (IsDrawingSecondPoint)
                {
                    Handles.DrawWireDisc(FirstPoint, Vector3.forward, 0.5f);
                    Handles.DrawWireDisc(position, Vector3.forward, 0.5f);
                    Vector3[] verts = new Vector3[4] {
                        new Vector3(FirstPoint.x, FirstPoint.y, 0),
                        new Vector3(FirstPoint.x, position.y, 0),
                        new Vector3(position.x, position.y, 0),
                        new Vector3(position.x, FirstPoint.y, 0),
                    };
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 0, 0, 0.2f), Color.red.Lighten(0.5f));
                }
                else
                {
                    Handles.DrawWireDisc(position, Vector3.forward, 0.5f);
                }
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
                IsDrawing = false;
            }

            public void StartDrawing()
            {
                if(IsDrawing) return;
                if(!IsValid) return;
                IsDrawing = true;
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