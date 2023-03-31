#if UNITY_EDITOR
using System;
using CoreLib;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
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
            if (target == null) return;
            try
            {
                Room room = (Room) target;
                string errorMsg = "";
                m_isCameraSelected = false;
                if (IsRoomValid(ref errorMsg))
                {
                    ChangeSelectedRoomCamera(room);
                }
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
                Debug.LogError($"Why is target {target} not a room!",target);
            }
           
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            try
            {
                Room room = (Room)target;
                if (m_selectedRoomCamera == room && !m_isCameraSelected)
                    ChangeSelectedRoomCamera(null);
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e);
                Debug.LogError($"Why is target {target} not a room!", target);
            }
            finally
            {
                base.OnDisable();
            }
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
            global::Buildings.Rooms.Rooms rooms = room.GetComponentInParent<global::Buildings.Rooms.Rooms>();
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

        public void OnSceneGUI()
        {
            Room room = (Room) target;
            if (room == null) return;
            global::Buildings.Rooms.Rooms rooms = room.GetComponentInParent<global::Buildings.Rooms.Rooms>();
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            var transform = rooms.Building.transform;
            if (room.isDynamic)
            {
                if (room.dynamicBody == null) return;
                transform = room.dynamicBody.transform;
            }

            bool snappingToGrid = Event.current.control;

            var bounds = room.Bounds;
            var center = transform.TransformPoint(room.roomBounds.center);
            
            DrawRoomArea(center, bounds, room, rooms.faceOpacity, rooms.outlineOpacity);
            DrawRoomBounds(rooms, center, room, transform, snappingToGrid);
        }

        private void DrawRoomBounds(global::Buildings.Rooms.Rooms rooms, Vector3 center, Room room, Transform transform, bool snapping =false)
        {
            m_BoundsHandle.center = center;
            m_BoundsHandle.size = room.roomBounds.size;

            m_BoundsHandle.midpointHandleSizeFunction = MidpointHandleSizeFunction;
            m_BoundsHandle.handleColor = room.roomColor;
            m_BoundsHandle.wireframeColor = room.roomColor.SetAlpha(rooms.outlineOpacity);
            Bounds newBounds = new Bounds();
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(room, "Changed Room Size");
                newBounds.center = transform.InverseTransformPoint(m_BoundsHandle.center);
                
                newBounds.size = (m_BoundsHandle.size);
                if (snapping)
                {
                    var bounds = newBounds;
                    var min = bounds.min;
                    var max = bounds.max;
                    var grid = transform.GetComponent<Grid>();
                    Debug.Assert(grid != null, "Building is missing grid!", transform);
                    if (grid == null)
                    {
                        return;
                    }
                    min =  grid.CellToLocal(grid.LocalToCell(min));
                    max = grid.CellToLocal(grid.LocalToCell(max));
                    bounds.SetMinMax(min, max);
                    newBounds = bounds;
                }
                room.roomBounds = newBounds;
            }
        }

        public static void DrawRoomArea(Vector3 center, Bounds bounds, Room room, float faceOpacity = 1f, float outlineOpacity = 1)
        {
            var rect = Rect.MinMaxRect(center.x - bounds.extents.x, center.y - bounds.extents.y, center.x + bounds.extents.x,
                center.y + bounds.extents.y);
            var color = room.roomColor;
            color.a = 0.25f * faceOpacity;
            var faceColor = color;
            faceColor.a = faceOpacity;
            var outlineColor = color;
            outlineColor.a = outlineOpacity;
            Handles.DrawSolidRectangleWithOutline(rect, faceColor, outlineColor);
        }
        public static void DrawRoomArea(global::Buildings.Rooms.Rooms rooms, Room room)
        {
            float faceOpacity = rooms.faceOpacity;
            float outlineOpacity = rooms.outlineOpacity;
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
            var faceColor = color;
            faceColor.a = faceOpacity;
            var outlineColor = color;
            outlineColor.a = outlineOpacity;
            Handles.DrawSolidRectangleWithOutline(rect, faceColor, outlineColor);
        }
        static float MidpointHandleSizeFunction(Vector3 position)
        {
            return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(position) * 2f;
        }
    }
}
#endif