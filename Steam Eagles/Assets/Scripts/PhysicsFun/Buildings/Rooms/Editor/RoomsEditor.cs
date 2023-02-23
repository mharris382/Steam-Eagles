
using System;
using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
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
        private void OnSceneGUI()
        {
            Rooms rooms = (Rooms) target;
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            foreach (var room in rooms.AllRooms)
            {
                RoomEditor.DrawRoomArea(rooms, room);
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
    }

    [CustomEditor(typeof(Room))]
    public class RoomEditor : OdinEditor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
        private static Room m_selectedRoomCamera;
        private bool m_isCameraSelected;

        bool IsSelectedRoomCamera(Room room)
        {
            return m_selectedRoomCamera == target;
        }
        public override void OnInspectorGUI()
        {
            
            Room room = (Room) target;
            string errorMsg = "";
            if (!IsRoomValid(ref errorMsg))
            {
                if (m_selectedRoomCamera == room) ChangeSelectedRoomCamera(null);
                EditorGUILayout.HelpBox(errorMsg, MessageType.Error);
                base.OnInspectorGUI();
                return;
            }

            if (IsSelectedRoomCamera(room) && GUILayout.Button("Disable Camera Preview"))
            {
                m_isCameraSelected = false;
                ChangeSelectedRoomCamera(null);
            }
            else if(!IsSelectedRoomCamera(room) && GUILayout.Button("Enable Camera Preview"))
            {
                ChangeSelectedRoomCamera(room);
                m_isCameraSelected = true;
            }
            base.OnInspectorGUI();

        }

        protected override void OnEnable()
        {
            Room room = (Room) target;
            string errorMsg = "";
            m_isCameraSelected = false;
            if (IsRoomValid(ref errorMsg))
            {
                ChangeSelectedRoomCamera(room);
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Room room = (Room) target;
            if(m_selectedRoomCamera == room && !m_isCameraSelected) 
                ChangeSelectedRoomCamera(null);
            base.OnDisable();
        }

        private void ChangeSelectedRoomCamera(Room o)
        {
            if (m_selectedRoomCamera != null && m_selectedRoomCamera.roomCamera != null)
            {
                m_selectedRoomCamera.roomCamera.gameObject.SetActive(false);
            }
            m_selectedRoomCamera = o;
            if (m_selectedRoomCamera != null  && m_selectedRoomCamera.roomCamera != null)
            {
                m_selectedRoomCamera.roomCamera.gameObject.SetActive(true);
            }
        }

        bool IsRoomValid(ref string msg)
        {
            Room room = (Room) target;
            
            if (room == null) return false;
            Rooms rooms = room.GetComponentInParent<Rooms>();
            if (rooms == null)
            {
                msg = "Room is not a child of a Rooms object";
                return false;
            }

            if (!rooms.HasBuilding)
            {
                msg = "Rooms object does not have a building";
                return false;
            }

            if (!room.isDynamic) return true;
            if (room.isDynamic && room.dynamicBody == null)
            {
                msg = "Room is dynamic but does not have a dynamic body";
                return false;
            }

            return true;
        }

        private void OnSceneGUI()
        {
            Room room = (Room) target;
            if (room == null) return;
            Rooms rooms = room.GetComponentInParent<Rooms>();
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            var transform = rooms.Building.transform;
            if (room.isDynamic)
            {
                if (room.dynamicBody == null) return;
                transform = room.dynamicBody.transform;
            }
            var bounds = room.Bounds;
            var center = transform.TransformPoint(room.roomBounds.center);
            
            DrawRoomArea(center, bounds, room);
            DrawRoomBounds(center, room, transform);
        }

        private void DrawRoomBounds(Vector3 center, Room room, Transform transform)
        {
            m_BoundsHandle.center = center;
            m_BoundsHandle.size = room.roomBounds.size;

            m_BoundsHandle.midpointHandleSizeFunction = MidpointHandleSizeFunction;
            m_BoundsHandle.handleColor = room.roomColor;
            m_BoundsHandle.wireframeColor = room.roomColor;
            Bounds newBounds = new Bounds();
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(room, "Changed Room Size");
                newBounds.center = transform.InverseTransformPoint(m_BoundsHandle.center);
                newBounds.size = (m_BoundsHandle.size);
                room.roomBounds = newBounds;
            }
        }

        public static void DrawRoomArea(Vector3 center, Bounds bounds, Room room)
        {
            var rect = Rect.MinMaxRect(center.x - bounds.extents.x, center.y - bounds.extents.y, center.x + bounds.extents.x,
                center.y + bounds.extents.y);
            var color = room.roomColor;
            color.a = 0.25f;
            Handles.DrawSolidRectangleWithOutline(rect, color, color);
        }
        public static void DrawRoomArea(Rooms rooms, Room room)
        {
            var transform = rooms.Building.transform;
            if (room.isDynamic)
            {
                if (room.dynamicBody == null) return;
                transform = room.dynamicBody.transform;
            }
            var center = transform.TransformPoint(room.roomBounds.center);
            var bounds = room.Bounds;
            var rect = Rect.MinMaxRect(center.x - bounds.extents.x, center.y - bounds.extents.y, center.x + bounds.extents.x,
                center.y + bounds.extents.y);
            var color = room.roomColor;
            color.a = 0.25f;
            Handles.DrawSolidRectangleWithOutline(rect, color, color);
        }
        static float MidpointHandleSizeFunction(Vector3 position)
        {
            return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(position) * 2f;
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